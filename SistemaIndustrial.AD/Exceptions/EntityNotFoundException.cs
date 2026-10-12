namespace SistemaIndustrial.AD.Exceptions;

public class EntityNotFoundException : Exception
{
    public string EntityName { get; }
    public int EntityId { get; }
    
    public EntityNotFoundException(string message) : base(message) 
    { 
        EntityName = "Entidad";
        EntityId = 0;
    }
    
    public EntityNotFoundException(string entityName, int id) 
        : base($"{entityName} con Id {id} no fue encontrado.")
    {
        EntityName = entityName;
        EntityId = id;
    }
}