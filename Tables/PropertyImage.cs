namespace Real_State.Tables
{
    public class PropertyImage
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public Property Property { get; set; }

        // Path to the image file
        public string ImagePath { get; set; }


    }
}
