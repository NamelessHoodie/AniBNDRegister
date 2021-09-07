using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using SoulsFormats;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

namespace AniBNDRegister
{
    class Program
    {
        private static readonly int baseTaeID = 5000000;
        static void Main(string[] args)
        {
            if (args.Length > 2)
            {
                if (File.Exists(args[0]))
                {
                    var aniBnd = BND4.Read(args[0]);

                    int[] taeIDsToRegister = Array.ConvertAll(args[1].Replace(" ", "").Split(','), int.Parse);
                    long[] taeSubIDsToRegister = Array.ConvertAll(args[2].Replace(" ", "").Split(','), long.Parse);
                    var aniBndFiles = aniBnd.Files;
                    var dummyTaeIDBinderFile = aniBndFiles.First(a => a.ID == 2 + baseTaeID);
                    Console.WriteLine("AniBNDRegister - Registering desired TAE and SUBTAE ID's");
                    Console.WriteLine("---------------------------------------------------------------");
                    foreach (var taeID in taeIDsToRegister)
                    {
                        Console.WriteLine("Working on TAE: a" + taeID.ToString("00"));
                        if (ListBinderFileContainsTaeID(taeID, aniBndFiles, out BinderFile binderFileResult))
                        {
                            try
                            {
                                TAE3 templateTae = TAE3.Read(dummyTaeIDBinderFile.Bytes);
                                templateTae.Animations = templateTae.Animations.Where(a => a.ID == 000000).ToList();
                                TAE3.Animation animTemplate = templateTae.Animations.First();

                                TAE3 taeObject = TAE3.Read(binderFileResult.Bytes);
                                foreach (var taeSubID in taeSubIDsToRegister)
                                {
                                    InsertAfterFirstLowerIDAnimation(taeSubID, ref taeObject.Animations, new TAE3.Animation(animTemplate) { AnimFileName = $"a{taeID.ToString("000")}_{taeSubID.ToString("000000")}.hkt", ID = taeSubID });
                                }
                                binderFileResult.Bytes = taeObject.Write();
                            }
                            catch (Exception exception)
                            {
                                Console.WriteLine(exception.ToString());
                                continue;
                            }
                        }
                        else
                        {
                            TAE3 templateTae = TAE3.Read(dummyTaeIDBinderFile.Bytes);
                            templateTae.Animations = templateTae.Animations.Where(a => a.ID == 000000).ToList();
                            TAE3.Animation animTemplate = templateTae.Animations.First();
                            templateTae.Animations.Remove(animTemplate);

                            foreach (var taeSubID in taeSubIDsToRegister)
                            {
                                InsertAfterFirstLowerIDAnimation(taeSubID, ref templateTae.Animations, new TAE3.Animation(animTemplate) { AnimFileName = $"a{taeID.ToString("000")}_{taeSubID.ToString("000000")}.hkt", ID = taeSubID });
                            }

                            var ll = new BinderFile(dummyTaeIDBinderFile.Flags, taeID + baseTaeID, $"N:\\FDP\\data\\INTERROOT_win64\\chr\\c0000\\tae\\a{taeID.ToString("00")}.tae", templateTae.Write());
                            InsertAfterFirstLowerIDBinderFile(taeID, ref aniBndFiles, ll);
                            aniBnd.Files = aniBndFiles;
                        }
                        Console.WriteLine();
                    }


                    if (File.Exists(args[0] + ".bak"))
                    {
                        File.Delete(args[0] + ".bak");
                    }
                    File.Move(args[0], args[0] + ".bak");
                    aniBnd.Write(args[0]);

                    Console.WriteLine("---------------------------------------------------------------");
                    Console.WriteLine("AniBNDRegister - Complete.");
                }
            }
            Console.ReadLine();
        }
        public static bool ListBinderFileContainsTaeID(int taeID, List<BinderFile> binderFileLst, out BinderFile fileContained)
        {
            var resultsQuery = binderFileLst.Where(a => a.ID == taeID + baseTaeID);
            if (resultsQuery.Any())
            {
                fileContained = resultsQuery.First();
                return true;
            }
            else
            {
                fileContained = null;
                return false;
            }
        }
        public static bool InsertAfterFirstLowerIDBinderFile(int taeID, ref List<BinderFile> binderList, BinderFile newBinderFile)
        {
            int ID = taeID + baseTaeID;
            if (!binderList.Any(item => item.ID == ID))
            {
                Console.WriteLine($"TAE file added ID: {taeID.ToString("00")}");
                binderList.Add(newBinderFile);
                binderList = binderList.OrderBy(item => item.ID).ToList();
                return true;
            }
            else
            {
                return false;
            }
        }
        static bool InsertAfterFirstLowerIDAnimation(long taeSubID, ref List<TAE3.Animation> animationList, TAE3.Animation newAnimation)
        {
            if (!animationList.Any(item => item.ID == taeSubID))
            {
                Console.WriteLine($"TAE subID registered ID: {taeSubID.ToString("000000")}");
                animationList.Add(newAnimation);
                animationList = animationList.OrderBy(item => item.ID).ToList();
                return true;
            }
            else
            {
                Console.WriteLine($"TAE subID already registered: {taeSubID.ToString("000000")}");
                return false;
            }
        }
    }
}
