﻿using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using ExplodePluginBase;

namespace Explode
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CurrentDirectory = "C:/Users";
        }

        // Creates a new plugin manager system and loads plugins
        PluginManager manager = new PluginManager(Directory.GetCurrentDirectory());
        private string directory;

        // this property controls directory changes and input validation
        private string CurrentDirectory
        {
            get { return directory; }
            set
            {
                // makes sure the new folder actually exists
                if (Directory.Exists(value))
                {
                    // if it doesn't end with a /, add one
                    if (value.Replace("\\", "/").EndsWith("/") == false)
                    {
                        directory = value.Replace("\\", "/") + "/";
                    }
                    else
                    {
                        directory = value.Replace("\\", "/");
                    }

                    // update the UI with the new directory
                    listView1.Items.Clear();
                    textBox1.Text = directory;
                    int index = 0;
                    foreach (string item in Directory.GetFileSystemEntries(directory))
                    {
                        listView1.Items.Add(item.Replace(CurrentDirectory, ""));
                        try
                        {
                            FileStream handle = File.OpenRead(item);
                            listView1.Items[index].SubItems.Add(handle.Length.ToString() + " B");
                            listView1.Items[index].SubItems.Add(getType(handle));
                            listView1.Items[index].SubItems.Add(Path.GetExtension(handle.Name));
                            handle.Close();
                        }
                        // this happens if it's a directory or can't be accessed
                        catch (UnauthorizedAccessException e)
                        {
                            if (Directory.Exists(item))
                            {
                                listView1.Items[index].SubItems.Add("");
                                listView1.Items[index].SubItems.Add("Folder");
                                listView1.Items[index].SubItems.Add(Path.GetExtension(item));
                            }
                        }
                        catch (Exception e)
                        {
                            continue;
                        }

                        index++;
                    }

                }
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) Keys.Enter)
            {
                CurrentDirectory = textBox1.Text;
            }
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            ListViewItem selectedItem = listView1.SelectedItems[0];
            if (Directory.Exists(CurrentDirectory + selectedItem.Text))
            {
                CurrentDirectory += selectedItem.Text;
            }
            else
            {
                foreach (IFileTypeBase type in manager.FileTypes)
                {
                    FileStream file = File.OpenRead(CurrentDirectory + selectedItem.Text);
                    if (type.CheckFileType(file) != null)
                    {
                        if (type.ExecuteFile(file) == 0)
                        {
                            System.Diagnostics.Process.Start(file.Name);
                        }
                        file.Close();
                        break;
                    }
                }
            }
        }

    private string getType(FileStream file)
        {
            string data = "Unknown type";
            foreach (IFileTypeBase plugin in manager.FileTypes)
            {
                string checkFileType = plugin.CheckFileType(file);
                if (checkFileType != null)
                {
                    data = checkFileType;
                    break;
                }
            }

            return data;
        }
    }
}