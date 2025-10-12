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
        // Методы для добавления
        public void AddStudent(Student student) => students.Add(student);
        public void AddTeacher(Teacher teacher) => teachers.Add(teacher);
        public void AddCourse(Course course) => courses.Add(course);