namespace CivicAlert.Api.Models;

public enum UserRole
{
    Citizen = 0,
    Official = 1,
    Admin = 2
}

public enum IssueType
{
    Pothole = 0,
    BrokenStreetlight = 1,
    IllegalDumping = 2,
    WaterLeak = 3,
    DamagedSidewalk = 4,
    TrafficSignal = 5
}

public enum IssueStatus
{
    Submitted = 0,
    UnderReview = 1,
    InProgress = 2,
    Resolved = 3,
    Rejected = 4
}
