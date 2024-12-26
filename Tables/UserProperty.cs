
using Real_State.Tables;

namespace Real_State.Tables
{
    [Flags]
    public enum RelationshipType
    {
        Viewed = 1,
        Saved = 2,
        Created = 4
    }

    public class UserProperty
    {
        public int PropertyId { get; set; }
        public Property Property { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public RelationshipType Type { get; set; }

        // Additional properties can go here
    }
}

//// When the user views a property
//userProperty.Type |= RelationshipType.Viewed;

//// When the user saves a property, it should also mark it as viewed
//userProperty.Type |= RelationshipType.Saved | RelationshipType.Viewed;

//// When the user creates a property, it should mark it as both created and viewed
//userProperty.Type |= RelationshipType.Created | RelationshipType.Viewed;





//// Check if the property was created by the user
//bool isCreated = (userProperty.Type & RelationshipType.Created) == RelationshipType.Created;

//// Check if the property was saved by the user
//bool isSaved = (userProperty.Type & RelationshipType.Saved) == RelationshipType.Saved;

//// Check if the property was viewed by the user
//bool isViewed = (userProperty.Type & RelationshipType.Viewed) == RelationshipType.Viewed;
