namespace ZLearn.API.Exceptions
{
    // Resource Exceptions
    public class NotFoundException : BaseException
    {
        public NotFoundException(string resourceType, string resourceId)
            : base($"{resourceType} with id '{resourceId}' was not found", ErrorCodes.NOT_FOUND)
        {
        }
        
        public NotFoundException(string message)
            : base(message, ErrorCodes.NOT_FOUND)
        {
        }
    }

    public class ResourceConflictException : BaseException
    {
        public ResourceConflictException(string message) 
            : base(message, ErrorCodes.RESOURCE_LOCKED)
        {
        }
    }

    public class BadRequestException : BaseException
    {
        public BadRequestException(string message)
            : base(message, ErrorCodes.BAD_REQUEST)
        {
        }
    }

    public class DuplicateEntryException : BaseException
    {
        public DuplicateEntryException(string entityType, string identifier)
            : base($"A {entityType} with the same {identifier} already exists", ErrorCodes.ALREADY_EXISTS)
        {
        }
    }

    public class ResourceDeletedException : BaseException
    {
        public ResourceDeletedException(string resourceType, string resourceId)
            : base($"The {resourceType} with id '{resourceId}' has been deleted", ErrorCodes.RESOURCE_DELETED)
        {
        }
    }
}