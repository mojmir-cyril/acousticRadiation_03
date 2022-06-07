#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//
//  Ansys:
//
using Ansys.Mechanical.DataModel.Converters;
using Ansys.Core.Units;
// using Ansys.Common.Interop.WBControls;
// using Ansys.Common.Interop.AnsCoreObjects;
using Ansys.Mechanical.DataModel.Enums;
using Ansys.ACT.Interfaces.Mesh;
using Ansys.ACT.Automation.Mechanical.BoundaryConditions;
using Ansys.ACT.Mechanical.Tools;
using Ansys.ACT.Common.Graphics;
using Ansys.ACT.Interfaces.UserObject;
using Ansys.ACT.Interfaces.Common;
using Ansys.ACT.Interfaces.Mechanical;
using Ansys.ACT.Automation.Mechanical;
using Ansys.ACT.Interfaces.Graphics;
using Ansys.ACT.Interfaces.Graphics.Entities;
using Ansys.ACT.Interfaces.Geometry; 
using Ansys.Core.Commands.DiagnosticCommands;
using Ansys.Mechanical;
using Ansys.Mechanical.DataModel;
using Ansys.ACT.Interfaces.Analysis;
using Ansys.Utilities;
using System.Reflection;
using SVSExceptionBase;
using System.IO;

namespace SVSEntityManagerF472.Help
{
    public partial class SFormHelp : Form
    { 
        public SEntityManager       em          { get; }
        public IMechanicalExtAPI    api         { get => em.api; }
        public SFormHelp(SEntityManager em)
        {
            try
            {
                this.em = em;
                //
                //
                //
                em.logger.Msg("SFormHelp(...)");
                //
                //  init:
                //
                InitializeComponent();
                //
                //  show:
                //
                Show(); 
            }
            catch (Exception err) { SExceptionBase.Throw(err, nameof(SFormHelp), nameof(SFormHelp)); }  // catch (Exception err) { throw new Exception($"SFormHelp(...): {err.Message}", err); }
        } 
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                string s1      = "N/A";
                string s2      = "N/A"; 
                string s3      = "N/A";                   
                try
                {
                    s1 = SAct.GetExtensionDir(api, @"help\em-examples.py");
                    s2 = SAct.GetExtensionDir(api, @"help\em-examples.cs");  
                    s3 = SAct.GetExtensionDir(api, @"help\em-install.py");  
                }
                catch { }
                if (!File.Exists(s1) && !File.Exists(s2) && !File.Exists(s3))
                { 
                    string l = Assembly.GetExecutingAssembly().Location;
                    string d = l.Substring(0, l.Count() - "SVSEntityManagerF472.dll".Count());
                    s1 = System.IO.Path.Combine(d, "em-examples.py");
                    s2 = System.IO.Path.Combine(d, "em-examples.cs");
                    s3 = System.IO.Path.Combine(d, "em-install.py");
                } 
                //
                //  log:
                //
                em.logger.Msg("Form1_Load(...)");
                em.logger.Msg(s1);
                em.logger.Msg(s2);
                em.logger.Msg(s3);
                //
                //  load:
                //
                string ReadFile(string f, RichTextBox r)
                {
                    string   data  = "N/A";
                    try 
                    {  
                        if (!File.Exists(f))
                        {
                            r.Text = $"Help for installation in file '{s3}' cannot be found. ";
                            return r.Text;
                        }
                        data  = File.ReadAllText(f);
                        string[] lines = data.Split('\n');
                        foreach (var line in lines)
                        {
                            if (line.StartsWith("#")) WriteLine(line, System.Drawing.Color.Green);
                            else                      WriteLine(line, System.Drawing.Color.Black); 
                        } 
                    } catch (Exception err) { r.Text = $"FILE: \n{s2}\nERROR: \n{err}"; }
                    return data;
                }
                void WriteLine(string s, System.Drawing.Color color)
                { 
                    richTextBox1.SelectionFont  = new Font("Courier New", 8, FontStyle.Regular);
                    richTextBox1.SelectionColor = color;
                    richTextBox1.AppendText($"{s}\n");  // .Replace("<br>", "\n")
                } 
                string install = ReadFile(s3, richTextBox1);
                ReadFile(s1, richTextBox1);
                ReadFile(s2, richTextBox2);  
                //
                //  about:
                //
                SetAbout(install);  
            }
            catch (Exception err) { SExceptionBase.Throw(err, nameof(SFormHelp), nameof(Form1_Load)); }  // catch (Exception err) { throw new Exception($"SFormHelp.Form1_Load(...): {err.Message}", err); }
        } 
        private void SetAbout(string install)
        {
            try 
            { 
                string html = $"<head>                                                                       " +
                              $"</head>                                                                      " +
                              $"<body>                                                                       " +
                              $"<pre>                                                                        " +
                              $"    \n <h2>SVS FEM Entity Manager (SVSEntityManager)</h2>                    " +
                              $"    \n <hr>                                                                  " +
                              $"    \n Main DLL     : SVSEntityManagerF472.dll (.NET Framework 7.4.2)        " +
                              $"    \n Version      : {SEntityManager.GetVersionString()}                    " +
                              $"    \n Developed by : Zdenek Cada (SVS FEM s.r.o.)                           " +
                              $"    \n Copyright    : SVS FEM s.r.o.                                         " +
                              $"    \n Target       : ANSYS Mechanical module                                " +
                              $"    \n Tested       : ANSYS Version 2021R2                                   " +
                              $"    \n <hr>                                                                  " +
                              $"    \n <b>Description:</b>                                                   " +
                              $"    \n Entity Manager is set of tools (objects, methods, ...) mainly for     " +
                              $"    \n selecting of geometrical entities (parts, bodies, faces, edges,       " +
                              $"    \n verticles) and mesh entities (nodes, elements, element faces).        " +
                              $"    \n <hr>                                                                  " +
                              $"    \n <b>Installation:</b>                                                  " +
                              $"    \n <i>\n{install}\n</i>                                                  " +
                              $"</pre>                                                                       " +
                              $"</body>                                                                      "; 
                browserAbout.DocumentText = html; 
            }
            catch (Exception err) { SExceptionBase.Throw(err, nameof(SFormHelp), nameof(SetAbout)); }  // catch (Exception err) { throw new Exception($"SFormHelp.SetAbout(...): {err.Message}", err); }
        }
    }
}
