using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;
using WebAPI.Services;

namespace WebAPI.Controllers;

// Direct messages. Whether a new conversation starts as a normal chat, as a request,
// or is refused, depends on the recipient's message privacy setting.
[ApiController]
[Route("[controller]")]
public class ConversationsController : ControllerBase
{
    private const int MaxMessageLength = 2000;

    private readonly IMessageRepository messageRepo;
    private readonly IUserRepository userRepo;

    public ConversationsController(IMessageRepository messageRepo, IUserRepository userRepo)
    {
        this.messageRepo = messageRepo;
        this.userRepo = userRepo;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ConversationDto>> GetConversations([FromQuery] int userId)
    {
        Dictionary<int, string> userNames = userRepo.GetMany().ToDictionary(u => u.Id, u => u.UserName);
        List<ConversationDto> dtos = messageRepo.GetConversations()
            .Where(c => c.UserOneId == userId || c.UserTwoId == userId)
            .OrderByDescending(c => c.LastMessageDate)
            .ToList()
            .Select(c => ToDto(c, userId, userNames))
            .ToList();
        return Ok(dtos);
    }

    [HttpPost]
    public async Task<ActionResult<ConversationDto>> StartConversation([FromBody] StartConversationDto request)
    {
        if (request.FromUserId == request.ToUserId)
        {
            throw new ArgumentException("You can't message yourself.");
        }

        await userRepo.GetSingleAsync(request.FromUserId);
        User recipient = await userRepo.GetSingleAsync(request.ToUserId);
        Dictionary<int, string> userNames = userRepo.GetMany().ToDictionary(u => u.Id, u => u.UserName);

        Conversation? existing = Find(request.FromUserId, request.ToUserId);
        if (existing is not null)
        {
            return Ok(ToDto(existing, request.FromUserId, userNames));
        }

        bool recipientFollowsSender = userRepo.GetFollowing(recipient.Id).Any(u => u.Id == request.FromUserId);
        bool accepted = (recipient.MessagePrivacy ?? "requests") switch
        {
            "everyone" => true,
            "following" when recipientFollowsSender => true,
            "following" => throw new InvalidOperationException($"{recipient.UserName} only accepts messages from people they follow."),
            "nobody" => throw new InvalidOperationException($"{recipient.UserName} doesn't accept messages."),
            _ => recipientFollowsSender
        };

        Conversation created = await messageRepo.AddConversationAsync(new Conversation
        {
            UserOneId = request.FromUserId,
            UserTwoId = request.ToUserId,
            StartedById = request.FromUserId,
            IsAccepted = accepted,
            CreatedDate = DateTime.Now,
            LastMessageDate = DateTime.Now
        });
        return Created($"/conversations/{created.Id}", ToDto(created, request.FromUserId, userNames));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ConversationDto>> GetConversation([FromRoute] int id, [FromQuery] int userId)
    {
        Conversation conversation = await GetForParticipantAsync(id, userId);
        Dictionary<int, string> userNames = userRepo.GetMany().ToDictionary(u => u.Id, u => u.UserName);
        return Ok(ToDto(conversation, userId, userNames));
    }

    // reading the messages also marks them as read for that user
    [HttpGet("{id:int}/messages")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages([FromRoute] int id, [FromQuery] int userId)
    {
        Conversation conversation = await GetForParticipantAsync(id, userId);
        List<MessageDto> messages = messageRepo.GetMessages(id).ToList().Select(DtoMapper.ToDto).ToList();

        if (conversation.UserOneId == userId) conversation.UserOneLastRead = DateTime.Now;
        else conversation.UserTwoLastRead = DateTime.Now;
        await messageRepo.UpdateConversationAsync(conversation);

        return Ok(messages);
    }

    [HttpPost("{id:int}/messages")]
    public async Task<ActionResult<MessageDto>> SendMessage([FromRoute] int id, [FromBody] SendMessageDto request)
    {
        Conversation conversation = await GetForParticipantAsync(id, request.SenderId);
        if (string.IsNullOrWhiteSpace(request.Body) && string.IsNullOrWhiteSpace(request.MediaUrl))
        {
            throw new ArgumentException("A message needs some text or an image.");
        }

        if (request.Body?.Length > MaxMessageLength)
        {
            throw new ArgumentException($"Messages can be at most {MaxMessageLength} characters.");
        }

        if (!string.IsNullOrWhiteSpace(request.MediaUrl))
        {
            MediaUrls.Validate(request.MediaUrl, request.MediaKind ?? "image");
        }

        // answering a request accepts it
        if (!conversation.IsAccepted && conversation.StartedById != request.SenderId)
        {
            conversation.IsAccepted = true;
        }

        Message created = await messageRepo.AddMessageAsync(new Message
        {
            ConversationId = id,
            SenderId = request.SenderId,
            Body = request.Body?.Trim() ?? "",
            MediaUrl = string.IsNullOrWhiteSpace(request.MediaUrl) ? null : request.MediaUrl,
            MediaKind = string.IsNullOrWhiteSpace(request.MediaUrl) ? null : request.MediaKind ?? "image",
            CreatedDate = DateTime.Now
        });

        conversation.LastMessageDate = created.CreatedDate;
        if (conversation.UserOneId == request.SenderId) conversation.UserOneLastRead = created.CreatedDate;
        else conversation.UserTwoLastRead = created.CreatedDate;
        await messageRepo.UpdateConversationAsync(conversation);

        return Created($"/conversations/{id}/messages/{created.Id}", DtoMapper.ToDto(created));
    }

    [HttpPost("{id:int}/accept")]
    public async Task<ActionResult> AcceptRequest([FromRoute] int id, [FromQuery] int userId)
    {
        Conversation conversation = await GetForParticipantAsync(id, userId);
        if (conversation.StartedById == userId)
        {
            throw new InvalidOperationException("Only the person who received the request can accept it.");
        }

        conversation.IsAccepted = true;
        await messageRepo.UpdateConversationAsync(conversation);
        return NoContent();
    }

    // declining a request and deleting a chat are the same thing
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteConversation([FromRoute] int id, [FromQuery] int userId)
    {
        await GetForParticipantAsync(id, userId);
        await messageRepo.DeleteConversationAsync(id);
        return NoContent();
    }

    private Conversation? Find(int a, int b)
    {
        return messageRepo.GetConversations().FirstOrDefault(c =>
            (c.UserOneId == a && c.UserTwoId == b) || (c.UserOneId == b && c.UserTwoId == a));
    }

    private async Task<Conversation> GetForParticipantAsync(int id, int userId)
    {
        Conversation conversation = await messageRepo.GetConversationAsync(id);
        if (conversation.UserOneId != userId && conversation.UserTwoId != userId)
        {
            throw new NotFoundException($"Conversation with ID '{id}' not found");
        }

        return conversation;
    }

    private ConversationDto ToDto(Conversation conversation, int viewerId, Dictionary<int, string> userNames)
    {
        int otherId = conversation.UserOneId == viewerId ? conversation.UserTwoId : conversation.UserOneId;
        DateTime? lastRead = conversation.UserOneId == viewerId ? conversation.UserOneLastRead : conversation.UserTwoLastRead;
        List<Message> messages = messageRepo.GetMessages(conversation.Id).ToList();
        Message? last = messages.LastOrDefault();

        string status = conversation.IsAccepted ? "active" : conversation.StartedById == viewerId ? "pending" : "request";
        return new ConversationDto
        {
            Id = conversation.Id,
            OtherUserId = otherId,
            OtherUserName = userNames.GetValueOrDefault(otherId, DtoMapper.DeletedUserName),
            Status = status,
            LastMessage = last is null ? null : string.IsNullOrWhiteSpace(last.Body) ? "sent an image" : last.Body,
            LastMessageDate = conversation.LastMessageDate,
            UnreadCount = messages.Count(m => m.SenderId != viewerId && (lastRead == null || m.CreatedDate > lastRead))
        };
    }
}
