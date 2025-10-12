using System;
using System.Collections.Generic;

namespace UniversityManagementSystem
{
    // Абстрактный класс Person для абстракции общих характеристик
    public abstract class Person
    {
        private static int nextId = 1;

        // Приватные поля для инкапсуляции
        private int id;
        private string name;
        private int age;
        private string contact;
        protected Person(string name, int age, string contact)
        {
            this.id = nextId++;
            this.name = name;
            this.age = age;
            this.contact = contact;
        }

        // Публичные свойства для доступа к данным (инкапсуляция)
        public int Id => id;
        public string Name => name;
        public int Age => age;
        public string Contact => contact;

        // Абстрактный метод для полиморфизма
        public abstract string GetRole();

        // Переопределение ToString для полиморфного вывода информации
        public override string ToString()
        {
            return $"ID: {Id}, Имя: {Name}, Возраст: {Age}, Контакт: {Contact}, Роль: {GetRole()}";
        }
    }// Класс Student, наследующий от Person
    public class Student : Person
    {
        // Приватное поле для списка курсов
        private List<Course> enrolledCourses = new List<Course>();

        public Student(string name, int age, string contact) : base(name, age, contact) { }

        // Публичный метод для записи на курс
        public void Enroll(Course course)
        {
            if (!enrolledCourses.Contains(course))
            {
                enrolledCourses.Add(course);
                course.AddStudent(this);
            }
        }
        // Публичный метод для получения списка курсов
        public List<Course> GetEnrolledCourses() => new List<Course>(enrolledCourses);

        // Реализация абстрактного метода (полиморфизм)
        public override string GetRole() => "Студент";

        // Переопределение ToString с дополнительной информацией
        public override string ToString()
        {
            return base.ToString() + $", Записанные курсы: {enrolledCourses.Count}";
        }
    }
    // Класс Teacher, наследующий от Person
    public class Teacher : Person
    {
        // Приватное поле для списка курсов
        private List<Course> taughtCourses = new List<Course>();

        public Teacher(string name, int age, string contact) : base(name, age, contact) { }

        // Публичный метод для назначения на курс
        public void AssignToCourse(Course course)
        {
            if (!taughtCourses.Contains(course))
            {
                taughtCourses.Add(course);
                course.AssignTeacher(this);
            }
        }
        // Публичный метод для получения списка курсов
        public List<Course> GetTaughtCourses() => new List<Course>(taughtCourses);

        // Реализация абстрактного метода (полиморфизм)
        public override string GetRole() => "Преподаватель";

        // Переопределение ToString с дополнительной информацией
        public override string ToString()
        {
            return base.ToString() + $", Преподаваемые курсы: {taughtCourses.Count}";
        }
    }
    // Класс Course
    public class Course
    {
        private static int nextId = 1;

        // Приватные поля
        private int id;
        private string name;
        private Teacher teacher;
        private List<Student> students = new List<Student>();

        public Course(string name)
        {
            this.id = nextId++;
            this.name = name;
        }

        // Публичные свойства
        public int Id => id;
        public string Name => name;
        public Teacher Teacher => teacher;

        // Метод для добавления студента
        public void AddStudent(Student student)
        {
            if (!students.Contains(student))
            {
                students.Add(student);
            }
        }

        // Метод для назначения преподавателя
        public void AssignTeacher(Teacher teacher)
        {
            this.teacher = teacher;
        }

        // Метод для получения списка студентов
        public List<Student> GetStudents() => new List<Student>(students);

        // Переопределение ToString
        public override string ToString()
        {
            string teacherInfo = teacher != null ? teacher.Name : "Не назначен";
            return $"ID: {Id}, Название: {Name}, Преподаватель: {teacherInfo}, Студенты: {students.Count}";
        }
    }
    // Класс University для управления всеми сущностями
    public class University
    {
        private List<Student> students = new List<Student>();
        private List<Teacher> teachers = new List<Teacher>();
        private List<Course> courses = new List<Course>();

        public void AddStudent(Student student) => students.Add(student);
        public void AddTeacher(Teacher teacher) => teachers.Add(teacher);
        public void AddCourse(Course course) => courses.Add(course);

        public Student FindStudentById(int id) => students.Find(s => s.Id == id);
        public Teacher FindTeacherById(int id) => teachers.Find(t => t.Id == id);
        public Course FindCourseById(int id) => courses.Find(c => c.Id == id);
        // Методы для вывода списков
        public void ListAllStudents()
        {
            Console.WriteLine("Все студенты:");
            foreach (var student in students)
            {
                Console.WriteLine(student);
            }
        }

        public void ListAllTeachers()
        {
            Console.WriteLine("Все преподаватели:");
            foreach (var teacher in teachers)
            {
                Console.WriteLine(teacher);
            }
        }

        public void ListAllCourses()
        {
            Console.WriteLine("Все курсы:");
            foreach (var course in courses)
            {
                Console.WriteLine(course);
            }
        }
    }
    // Основной класс программы
    class Program
    {
        static void Main(string[] args)
        {
            University university = new University();
            bool running = true;

            while (running)
            {
                Console.WriteLine("\nМеню системы управления университетом:");
                Console.WriteLine("1. Добавить студента");
                Console.WriteLine("2. Просмотреть информацию о студенте");
                Console.WriteLine("3. Записать студента на курс");
                Console.WriteLine("4. Просмотреть курсы студента");
                Console.WriteLine("5. Добавить преподавателя");
                Console.WriteLine("6. Просмотреть информацию о преподавателе");
                Console.WriteLine("7. Назначить преподавателя на курс");
                Console.WriteLine("8. Создать курс");
                Console.WriteLine("9. Просмотреть информацию о курсе");
                Console.WriteLine("10. Просмотреть студентов на курсе");
                Console.WriteLine("11. Список всех студентов");
                Console.WriteLine("12. Список всех преподавателей");
                Console.WriteLine("13. Список всех курсов");
                Console.WriteLine("14. Выход");
                Console.Write("Введите ваш выбор: ");

                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    switch (choice)
                    {
                        case 1:
                            AddStudent(university);
                            break;
                        case 2:
                            ViewStudentInfo(university);
                            break;
                        case 3:
                            EnrollStudentToCourse(university);
                            break;
                        case 4:
                            ViewStudentCourses(university);
                            break;
                        case 5:
                            AddTeacher(university);
                            break;
                        case 6:
                            ViewTeacherInfo(university);
                            break;
                        case 7:
                            AssignTeacherToCourse(university);
                            break;
                        case 8:
                            CreateCourse(university);
                            break;
                        case 9:
                            ViewCourseInfo(university);
                            break;
                        case 10:
                            ViewStudentsInCourse(university);
                            break;
                        case 11:
                            university.ListAllStudents();
                            break;
                        case 12:
                            university.ListAllTeachers();
                            break;
                        case 13:
                            university.ListAllCourses();
                            break;
                        case 14:
                            running = false;
                            break;
                        default:
                            Console.WriteLine("Неверный выбор. Пожалуйста, попробуйте снова.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Неверный ввод. Пожалуйста, введите число.");
                }
            }
        }
        // Вспомогательные методы для меню
        private static void AddStudent(University university)
        {
            Console.Write("Введите имя: ");
            string name = Console.ReadLine();
            Console.Write("Введите возраст: ");
            int age = int.Parse(Console.ReadLine());
            Console.Write("Введите контакт: ");
            string contact = Console.ReadLine();
            Student student = new Student(name, age, contact);
            university.AddStudent(student);
            Console.WriteLine("Студент добавлен успешно.");
        }
        private static void ViewStudentInfo(University university)
        {
            Console.Write("Введите ID студента: ");
            int id = int.Parse(Console.ReadLine());
            Student student = university.FindStudentById(id);
            if (student != null)
            {
                Console.WriteLine(student);
            }
            else
            {
                Console.WriteLine("Студент не найден.");
            }
        }
        private static void EnrollStudentToCourse(University university)
        {
            Console.Write("Введите ID студента: ");
            int studentId = int.Parse(Console.ReadLine());
            Student student = university.FindStudentById(studentId);
            if (student == null)
            {
                Console.WriteLine("Студент не найден.");
                return;
            }

            Console.Write("Введите ID курса: ");
            int courseId = int.Parse(Console.ReadLine());
            Course course = university.FindCourseById(courseId);
            if (course == null)
            {
                Console.WriteLine("Курс не найден.");
                return;
            }

            student.Enroll(course);
            Console.WriteLine("Студент записан успешно.");
        }
        private static void ViewStudentCourses(University university)
        {
            Console.Write("Введите ID студента: ");
            int id = int.Parse(Console.ReadLine());
            Student student = university.FindStudentById(id);
            if (student != null)
            {
                Console.WriteLine($"Курсы для {student.Name}:");
                foreach (var course in student.GetEnrolledCourses())
                {
                    Console.WriteLine(course);
                }
            }
            else
            {
                Console.WriteLine("Студент не найден.");
            }
        }
        private static void AddTeacher(University university)
        {
            Console.Write("Введите имя: ");
            string name = Console.ReadLine();
            Console.Write("Введите возраст: ");
            int age = int.Parse(Console.ReadLine());
            Console.Write("Введите контакт: ");
            string contact = Console.ReadLine();
            Teacher teacher = new Teacher(name, age, contact);
            university.AddTeacher(teacher);
            Console.WriteLine("Преподаватель добавлен успешно.");
        }
        private static void ViewTeacherInfo(University university)
        {
            Console.Write("Введите ID преподавателя: ");
            int id = int.Parse(Console.ReadLine());
            Teacher teacher = university.FindTeacherById(id);
            if (teacher != null)
            {
                Console.WriteLine(teacher);
            }
            else
            {
                Console.WriteLine("Преподаватель не найден.");
            }
        }
        private static void AssignTeacherToCourse(University university)
        {
            Console.Write("Введите ID преподавателя: ");
            int teacherId = int.Parse(Console.ReadLine());
            Teacher teacher = university.FindTeacherById(teacherId);
            if (teacher == null)
            {
                Console.WriteLine("Преподаватель не найден.");
                return;
            }

            Console.Write("Введите ID курса: ");
            int courseId = int.Parse(Console.ReadLine());
            Course course = university.FindCourseById(courseId);
            if (course == null)
            {
                Console.WriteLine("Курс не найден.");
                return;
            }

            teacher.AssignToCourse(course);
            Console.WriteLine("Преподаватель назначен успешно.");
        }