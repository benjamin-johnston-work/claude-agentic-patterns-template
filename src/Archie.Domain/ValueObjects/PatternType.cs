namespace Archie.Domain.ValueObjects;

public enum PatternType
{
    // Creational Patterns
    Singleton,
    Factory,
    AbstractFactory,
    Builder,
    
    // Structural Patterns
    Adapter,
    Decorator,
    Facade,
    Proxy,
    
    // Behavioral Patterns
    Observer,
    Strategy,
    Command,
    Template,
    
    // Architectural Patterns
    MVC,
    Repository,
    UnitOfWork,
    DependencyInjection,
    LayeredArchitecture,
    
    // Domain-Driven Design
    Aggregate,
    Entity,
    ValueObject,
    DomainService,
    
    // Microservices
    APIGateway,
    ServiceMesh,
    EventSourcing,
    CQRS
}