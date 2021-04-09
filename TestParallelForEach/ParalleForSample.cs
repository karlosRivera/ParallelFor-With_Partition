using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskPartitionExample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<Person> persons = GetPerson();
            int ageTotal = 0;

            Parallel.ForEach
            (
                persons,
                () => 0,
                (person, loopState, subtotal) => subtotal + person.Age,
                (subtotal) => Interlocked.Add(ref ageTotal, subtotal)
            );

            MessageBox.Show(ageTotal.ToString());
        }

        private List<Person> GetPerson()
        {
            List<Person> p = new List<Person>
            {
                new Person() { Id = 0, Name = "Artur", Age = 5 },
                new Person() { Id = 1, Name = "Edward", Age = 10 },
                new Person() { Id = 2, Name = "Krzysiek", Age = 20 },
                new Person() { Id = 3, Name = "Piotr", Age = 15 },
                new Person() { Id = 4, Name = "Adam", Age = 10 }
            };

            return p;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            int[] nums = Enumerable.Range(0, 7).ToArray();
            long total = 0;

            // First type parameter is the type of the source elements
            // Second type parameter is the type of the thread-local variable (partition subtotal)
            Parallel.ForEach<int, long>(nums, // source collection
                                            new ParallelOptions { MaxDegreeOfParallelism = 1 },
                                            () => 0, // method to initialize the local variable
                                            (j, loop, subtotal) => // method invoked by the loop on each iteration
                                            {
                                                subtotal += j; //modify local variable
                                                return subtotal; // value to be passed to next iteration
                                            },
                                            // Method to be executed when each partition has completed.
                                            // finalResult is the final value of subtotal for a particular partition.
                                            (finalResult) => 
                                            {
                                                Interlocked.Add(ref total, finalResult);
                                            }
                                        );

            //Console.WriteLine("The total from Parallel.ForEach is {0:N0}", total);
            MessageBox.Show(total.ToString());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<Person> persons = GetPerson();
            var persons1 = new List<Person>();
            var locker = new object();
            Parallel.ForEach(
                persons,
                () => new List<Person>(), // initialize aggregate per thread 
                (person, loopState, subtotal) =>
                {
                    subtotal.Add(person); // add current thread element to aggregate 
                    return subtotal; // return current thread aggregate
                },
                p => // action to combine all threads results
                {
                    lock (locker) // lock, cause List<T> is not a thread safe collection
                    {
                        persons1.AddRange(p);
                    }
                }
            );
        }
    }

    class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
