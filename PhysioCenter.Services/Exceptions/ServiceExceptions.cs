namespace PhysioCenter.Services.Exceptions;

/// <summary>The requested record does not exist (HTTP 404).</summary>
public class NotFoundException(string entity, object key)
    : Exception($"{entity} with id {key} was not found.");

/// <summary>The request is valid, but clashes with existing data (HTTP 409).</summary>
public class ConflictException(string message) : Exception(message);

/// <summary>The request breaks a business rule, e.g. a referenced record does not exist (HTTP 400).</summary>
public class BusinessRuleException(string message) : Exception(message);
