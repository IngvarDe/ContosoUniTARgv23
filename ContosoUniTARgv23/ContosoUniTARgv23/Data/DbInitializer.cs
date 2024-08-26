namespace ContosoUniTARgv23.Data
{
    public class DbInitializer
    {
        public static void Intialize(SchoolContext context)
        {
            context.Database.EnsureCreated();

            //look for any students
            if (context.Students.Any())
            {
                return;
            }


        }
    }
}
