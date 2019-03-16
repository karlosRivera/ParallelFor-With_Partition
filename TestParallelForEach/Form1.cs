using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace TestParallelForEach
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // map xml files from xml folder from this project
            textBox1.Text = "";
            DataSet ds = new DataSet();
            WeightageRowNumber brokerRowWeightageRowNumber = null;
            WeightageRowNumber ConsensusRowWeightageRowNumber = null;
            WeightageRowNumber QcRowWeightageRowNumber = null;
            List<WeightageRowNumber> WeightageRowNumberall = new List<WeightageRowNumber>();

            //string distinctbrokername = "3A-P1,4E,BW,BW-P1,CL,DD-P1,FB,FB-C1,GS,MD,MD-P1,ML,SB,UA";
            string distinctbrokername = "3A-P1,4E";
            List<string> DistinctBroker = distinctbrokername.Split(',').ToList<string>();

            //var ConsensusRowData = objQcVerticalViewNewProcess.ConsensusRowAll;
            //var qcTrueDistin = ConsensusRowData.Where(y => y.IsQcCheck).DistinctBy(x => new { x.TabName, x.StandardLineItem }).ToList();

            
            ds.ReadXml(@"C:\Users\TRIDIP\Downloads\OrderWiseLineItem.xml");
            List<WeightageRowNumber> OrderWiseLineItem = ds.Tables[0].DataTableToList<WeightageRowNumber>();

            //OrderWiseLineItem.AsParallel().AsOrdered().ForAll(x=>
            //    {
            //        x.Section
            //    }
            //    )


            object _lock = new object();
            int rowNumber = 0;

            Parallel.ForEach(OrderWiseLineItem,
                new ParallelOptions { MaxDegreeOfParallelism = 10 },
                data =>
                {
                    // string Li = data1.LineItem;
                    string section = data.Section;
                    string Li = data.Lineitem;

                    if (!String.IsNullOrEmpty(section) && !String.IsNullOrEmpty(Li))
                    {
                        Parallel.ForEach<string, int>(
                        DistinctBroker,
                        new ParallelOptions { MaxDegreeOfParallelism = 10 },
                        () => rowNumber,
                        (broker, state, localrownumber) =>
                        {
                            lock (_lock)
                            {
                                // for broker row .... weightage 1 (no color)
                                localrownumber = Interlocked.Increment(ref rowNumber);
                                brokerRowWeightageRowNumber = new WeightageRowNumber();
                                brokerRowWeightageRowNumber.Section = section;
                                brokerRowWeightageRowNumber.Lineitem = Li;
                                brokerRowWeightageRowNumber.Broker = broker;
                                brokerRowWeightageRowNumber.RowNumber = localrownumber;
                                brokerRowWeightageRowNumber.Weightage = 1;

                                WeightageRowNumberall.Add(brokerRowWeightageRowNumber);
                            }

                            return rowNumber;
                        },
                        (incrementedRowNumber) =>
                        {
                            lock (_lock)
                            {
                                // for broker row .... weightage 2 (red color)
                                ConsensusRowWeightageRowNumber = new WeightageRowNumber();
                                ConsensusRowWeightageRowNumber.Section = section;
                                ConsensusRowWeightageRowNumber.Lineitem = Li;
                                ConsensusRowWeightageRowNumber.Broker = "";
                                ConsensusRowWeightageRowNumber.RowNumber = Interlocked.Increment(ref rowNumber);
                                ConsensusRowWeightageRowNumber.Weightage = 2;
                                WeightageRowNumberall.Add(ConsensusRowWeightageRowNumber);
                            }

                            // for QC Check row .... weightage 3, if any  (yellow color)
                            //if (qcTrueDistin.Any(x => x.TabName.Equals(section) && x.StandardLineItem.Equals(Li)))
                            //{
                            /*
                            Parallel.ForEach(DistinctBroker, new ParallelOptions { MaxDegreeOfParallelism = 1 }, broker =>
                                {
                                    int localrownumber = Interlocked.Increment(ref rowNumber);
                                    QcRowWeightageRowNumber = new WeightageRowNumber();
                                    QcRowWeightageRowNumber.Section = section;
                                    QcRowWeightageRowNumber.Lineitem = Li;
                                    QcRowWeightageRowNumber.Broker = broker;
                                    QcRowWeightageRowNumber.RowNumber = localrownumber;
                                    QcRowWeightageRowNumber.Weightage = 3;

                                    lock (_lock)
                                    {
                                        WeightageRowNumberall.Add(QcRowWeightageRowNumber);
                                    }

                                    Console.WriteLine(QcRowWeightageRowNumber.Section
                                        + " " + QcRowWeightageRowNumber.Lineitem
                                        + " " + QcRowWeightageRowNumber.Broker
                                        + " " + QcRowWeightageRowNumber.Weightage
                                        + " " + QcRowWeightageRowNumber.RowNumber);

                                });
                              */
                            //}

                        });
                    }
                });

            MessageBox.Show("done");

                WeightageRowNumberall.ForEach(x => 
                {
                    textBox1.Text += (x.Section + " " + x.Lineitem + " " + x.Broker + " " + " Weitage " + x.Weightage + " Rno " + x.RowNumber) + Environment.NewLine;
                }); 
        }
    }

    public class WeightageRowNumber
    {

        public WeightageRowNumber()
        {
            this.Broker = string.Empty;
            this.Section = string.Empty;
            this.Lineitem = string.Empty;
            this.RowNumber = -1;
            this.Weightage = 0;
            this.Id = "-1";
        }

        public string Broker { get; set; }
        public string Section { get; set; }
        public string Lineitem { get; set; }
        public int RowNumber { get; set; }
        public int Weightage { get; set; }

        public string Id { get; set; }

    }


    public static class utility
    {
        public static List<T> DataTableToList<T>(this DataTable table) where T : new()
        {
            List<T> list = new List<T>();
            var typeProperties = typeof(T).GetProperties().Select(propertyInfo => new
            {
                PropertyInfo = propertyInfo,
                Type = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType
            }).ToList();

            foreach (var row in table.Rows.Cast<DataRow>())
            {
                T obj = new T();
                foreach (var typeProperty in typeProperties)
                {
                    object value = row[typeProperty.PropertyInfo.Name];
                    object safeValue = value == null || DBNull.Value.Equals(value)
                        ? null
                        : Convert.ChangeType(value, typeProperty.Type);

                    typeProperty.PropertyInfo.SetValue(obj, safeValue, null);
                }
                list.Add(obj);
            }
            return list;
        }

    }
}
