using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json;

namespace UniversityManagementSystem
{
    // Interface for entities that can be saved and loaded
    public interface IDataPersistable<T>
    {
        void Save(string filePath);
        T Load(string filePath);
    }

    // Custom attribute for marking courses
    [AttributeUsage(AttributeTargets.Class)]
    public class CourseAttribute : Attribute
    {
        public string Description { get; }

        public CourseAttribute(string description)
        {
            Description = description;
        }
    }

    // Custom exception for student-related errors
    public class StudentException : Exception
    {
        public StudentException(string message) : base(message)
        {
        }
    }

    // Enum for departments
    public enum Department
    {
        ComputerScience,
        BBA,
        English
    }

    // Enum for degrees
    public enum Degree
    {
        BSC,
        BBA,
        BA,
        MSC,
        MBA,
        MA
    }

    // Class representing a Semester
    public class Semester
    {
        public string SemesterCode { get; set; }
        public string Year { get; set; }
    }

    // Base class for Course
    public abstract class Course
    {
        public string CourseID { get; set; }
        public string CourseName { get; set; }
        public string InstructorName { get; set; }
        public int Credits { get; set; }

        public abstract string GetCourseInfo();
    }

    // Derived class for Online Course
    public class OnlineCourse : Course
    {
        public string Platform { get; set; }

        public override string GetCourseInfo()
        {
            return $"{CourseName} (Online)";
        }
    }

    // Derived class for Offline Course
    public class OfflineCourse : Course
    {
        public string Location { get; set; }

        public override string GetCourseInfo()
        {
            return $"{CourseName} (Offline)";
        }
    }

    // Class to represent student data
    public class Student : IDataPersistable<Student>
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string StudentID { get; set; }
        public Semester JoiningBatch { get; set; }
        public Department Department { get; set; }
        public Degree Degree { get; set; }
        public List<Semester> SemestersAttended { get; set; }
        public List<Course> Courses { get; set; }

        // Property with internal access
        internal string InternalData { get; set; }

        // Property with getter-only auto-property syntax
        public string FullName => $"{FirstName} {MiddleName} {LastName}";

        // Property with nullable type
        public int? NullableProperty { get; set; }

        // Delegate for handling events
        public delegate void StudentEventHandler(Student student);

        // Event for student enrollment
        public event StudentEventHandler Enrolled;

        // Method to raise the Enrolled event
        public void OnEnrolled()
        {
            Enrolled?.Invoke(this);
        }

        // Method to save student data to JSON file
        public void Save(string filePath)
        {
            try
            {
                string json = JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Serialization error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw new StudentException($"Failed to save student data: {ex.Message}");
            }
        }

        // Method to load student data from JSON file
        public Student Load(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    return JsonConvert.DeserializeObject<Student>(json);
                }
                else
                {
                    throw new FileNotFoundException("Student data file not found.");
                }
            }
            catch (Exception ex)
            {
                throw new StudentException($"Failed to load student data: {ex.Message}");
            }
        }
    }

    // Static class for extension methods
    public static class StudentExtensions
    {
        // Extension method for displaying student details
        public static void DisplayDetails(this Student student)
        {
            Console.WriteLine($"Name: {student.FullName}");
            Console.WriteLine($"Student ID: {student.StudentID}");
            Console.WriteLine($"Joining Batch: {student.JoiningBatch.SemesterCode} {student.JoiningBatch.Year}");
            Console.WriteLine($"Department: {student.Department}");
            Console.WriteLine($"Degree: {student.Degree}");
        }
    }

    class Program
    {
        static string studentsDirectory = "StudentsData";
        static List<Student> students = new List<Student>();

        static void Main(string[] args)
        {
            if (!Directory.Exists(studentsDirectory))
            {
                Directory.CreateDirectory(studentsDirectory);
            }

            try
            {
                LoadData();

                bool exit = false;
                while (!exit)
                {
                    Console.WriteLine("1. Add New Student");
                    Console.WriteLine("2. View Student Details");
                    Console.WriteLine("3. Delete Student");
                    Console.WriteLine("4. Exit");
                    Console.WriteLine("Enter your choice:");

                    switch (Console.ReadLine())
                    {
                        case "1":
                            AddNewStudent();
                            break;
                        case "2":
                            ViewStudentDetails();
                            break;
                        case "3":
                            DeleteStudent();
                            break;
                        case "4":
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Invalid choice.");
                            break;
                    }
                }
            }
            catch (StudentException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        static void LoadData()
        {
            foreach (string filePath in Directory.GetFiles(studentsDirectory, "*.json"))
            {
                try
                {
                    Student student = new Student().Load(filePath);
                    students.Add(student);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load student data from {filePath}: {ex.Message}");
                }
            }
        }

        static void SaveStudentData(Student student)
        {
            string filePath = Path.Combine(studentsDirectory, $"{student.StudentID}.json");
            student.Save(filePath);
        }

        static void DeleteStudentData(Student student)
        {
            string filePath = Path.Combine(studentsDirectory, $"{student.StudentID}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        static void AddNewStudent()
        {
            Student newStudent = new Student();

            Console.Write("Enter First Name: ");
            newStudent.FirstName = Console.ReadLine();

            Console.Write("Enter Middle Name: ");
            newStudent.MiddleName = Console.ReadLine();

            Console.Write("Enter Last Name: ");
            newStudent.LastName = Console.ReadLine();

            Console.Write("Enter Student ID (in format XXX-XXX-XXX): ");
            newStudent.StudentID = Console.ReadLine();

            Console.Write("Enter Joining Batch (Semester Code): ");
            string semesterCode = Console.ReadLine();

            Console.Write("Enter Joining Batch (Year): ");
            string year = Console.ReadLine();

            newStudent.JoiningBatch = new Semester { SemesterCode = semesterCode, Year = year };

            Console.Write("Enter Department (0 for ComputerScience, 1 for BBA, 2 for English): ");
            newStudent.Department = (Department)int.Parse(Console.ReadLine());

            Console.Write("Enter Degree (0 for BSC, 1 for BBA, 2 for BA, 3 for MSC, 4 for MBA, 5 for MA): ");
            newStudent.Degree = (Degree)int.Parse(Console.ReadLine());

            students.Add(newStudent);
            SaveStudentData(newStudent);
            Console.WriteLine("Student added successfully.");
        }

        static void ViewStudentDetails()
        {
            if (students.Count == 0)
            {
                Console.WriteLine("No student data available.");
                return;
            }

            Console.Write("Enter Student ID to view details: ");
            string studentID = Console.ReadLine();

            Student student = students.Find(s => s.StudentID == studentID);
            if (student != null)
            {
                student.DisplayDetails(); // Using the extension method here
            }
            else
            {
                Console.WriteLine("Student not found.");
            }
        }

        static void DeleteStudent()
        {
            if (students.Count == 0)
            {
                Console.WriteLine("No student data available.");
                return;
            }

            Console.Write("Enter Student ID to delete: ");
            string studentID = Console.ReadLine();

            Student student = students.Find(s => s.StudentID == studentID);
            if (student != null)
            {
                students.Remove(student);
                DeleteStudentData(student);
                Console.WriteLine("Student deleted successfully.");
            }
            else
            {
                Console.WriteLine("Student not found.");
            }
        }
    }
}
