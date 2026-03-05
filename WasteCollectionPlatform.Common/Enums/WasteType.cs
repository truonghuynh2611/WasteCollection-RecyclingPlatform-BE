namespace WasteCollectionPlatform.Common.Enums;

/// <summary>
/// Types of waste that can be reported
/// </summary>
public enum WasteType
{
    /// <summary>
    /// Plastic waste (bottles, bags, containers)
    /// </summary>
    Plastic = 0,
    
    /// <summary>
    /// Paper and cardboard waste
    /// </summary>
    Paper = 1,
    
    /// <summary>
    /// Metal waste (cans, foil)
    /// </summary>
    Metal = 2,
    
    /// <summary>
    /// Organic/food waste
    /// </summary>
    Organic = 3,
    
    /// <summary>
    /// Electronic waste (batteries, devices)
    /// </summary>
    Electronic = 4,
    
    /// <summary>
    /// Glass waste
    /// </summary>
    Glass = 5,
    
    /// <summary>
    /// Hazardous waste
    /// </summary>
    Hazardous = 6,
    
    /// <summary>
    /// Mixed or other waste types
    /// </summary>
    Mixed = 7
}
