namespace Matrix.Data.Models.TimeValidation;

public class ValidationUnit
{
    public List<int> ClockFacesPresent { get; internal set; }
    
    public bool IsValid => ClockFacesPresent != null && ClockFacesPresent.Count == 1;

    public void TryValidateFace(int clockFaceId)
    {
        if (ClockFacesPresent == null)
        {
            ClockFacesPresent = new List<int>();
        }
        
        ClockFacesPresent.Add(clockFaceId);
    }
}