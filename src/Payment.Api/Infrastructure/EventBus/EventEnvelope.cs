using System.Text.Json;

namespace Payment.Api.Infrastructure.EventBus;

public record EventEnvelope(string EventType, JsonElement Data);