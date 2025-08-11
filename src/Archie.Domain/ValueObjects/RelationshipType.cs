namespace Archie.Domain.ValueObjects;

public enum RelationshipType
{
    // Structural relationships
    Inheritance,        // Class A extends B
    Implementation,     // Class A implements Interface B
    Composition,        // Class A contains/owns B
    Aggregation,        // Class A uses B
    Association,        // Class A references B
    
    // Behavioral relationships
    Calls,              // Method A calls Method B
    Uses,               // A uses B's functionality
    Depends,            // A depends on B
    Creates,            // A creates instances of B
    Returns,            // A returns type B
    Accepts,            // A accepts parameter of type B
    
    // Architectural relationships
    LayerDependency,    // A is in higher layer than B
    ServiceConsumption, // A consumes service B
    EventPublishing,    // A publishes event B consumes
    EventSubscription,  // A subscribes to events from B
    
    // Cross-repository relationships
    SharedInterface,    // Both implement same interface pattern
    SimilarConcept,     // Semantically similar entities
    SharedDependency,   // Both depend on same external component
    
    // Pattern relationships
    PatternInstance,    // A is instance of pattern P
    PatternComponent    // A is component of pattern P
}