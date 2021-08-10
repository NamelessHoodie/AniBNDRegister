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
                    int[] taeSubIDsToRegister = Array.ConvertAll(args[2].Replace(" ", "").Split(','), int.Parse);
                    var aniBndFiles = aniBnd.Files;
                    var dummyTaeIDBinderFile = aniBndFiles.First(a => a.ID == 2 + baseTaeID);
                    foreach (var taeID in taeIDsToRegister)
                    {
                        Console.WriteLine("Working on TAE: a" + taeID.ToString("00"));
                        if (ListBinderFileContainsTaeID(taeID, aniBndFiles, out BinderFile binderFileResult))
                        {
                            TAE3 templateTae = TAE3.Read(dummyTaeIDBinderFile.Bytes);
                            templateTae.Animations = templateTae.Animations.Where(a => a.ID == 000000).ToList();
                            TAE3.Animation animTemplate = templateTae.Animations.First();

                            TAE3 taeObject = TAE3.Read(binderFileResult.Bytes);
                            foreach (var taeSubID in taeSubIDsToRegister)
                            {
                                if (!taeObject.Animations.Any(subTae => subTae.ID == taeSubID))
                                {
                                    InsertAfterFirstLowerIDAnimation(taeSubID, taeObject.Animations, new TAE3.Animation(animTemplate) { AnimFileName = $"a{taeID.ToString("000")}_{taeSubID.ToString("000000")}.hkt", ID = taeSubID });
                                }
                            }
                            binderFileResult.Bytes = taeObject.Write();
                        }
                        else
                        {
                            TAE3 templateTae = TAE3.Read(dummyTaeIDBinderFile.Bytes);
                            templateTae.Animations = templateTae.Animations.Where(a => a.ID == 000000).ToList();
                            TAE3.Animation animTemplate = templateTae.Animations.First();

                            foreach (var taeSubID in taeSubIDsToRegister)
                            {
                                if (!templateTae.Animations.Any(subTae => subTae.ID == taeSubID))
                                {
                                    InsertAfterFirstLowerIDAnimation(taeSubID, templateTae.Animations, new TAE3.Animation(animTemplate) { AnimFileName = $"a{taeID.ToString("000")}_{taeSubID.ToString("000000")}.hkt", ID = taeSubID });
                                }
                            }

                            templateTae.Animations.Remove(animTemplate);
                            var ll = new BinderFile(dummyTaeIDBinderFile.Flags, taeID + baseTaeID, $"N:\\FDP\\data\\INTERROOT_win64\\chr\\c0000\\tae\\a{taeID.ToString("00")}.tae", templateTae.Write());
                            InsertAfterFirstLowerIDBinderFile(taeID, aniBndFiles, ll);
                        }
                        Console.WriteLine();
                    }
                    File.Delete(args[0] + ".bak");
                    File.Move(args[0], args[0] + ".bak");
                    aniBnd.Write(args[0]);
                    //foreach (var tae in aniBndFiles)
                    //{
                    //    Console.WriteLine(tae.ID);
                    //    Console.WriteLine(tae.Name);
                    //    Console.WriteLine();
                    //    if (TAE3.Is(tae.Bytes))
                    //    {
                    //        foreach (var animation in TAE3.Read(tae.Bytes).Animations)
                    //        {
                    //            Console.WriteLine("     :" + animation.ID);
                    //            Console.WriteLine("     :" + animation.AnimFileName);
                    //        }
                    //    }
                    //    Console.WriteLine();
                    //}
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
        public static bool InsertAfterFirstLowerIDBinderFile(int taeID, List<BinderFile> binderList, BinderFile newBinderFile)
        {
            int ID = taeID + baseTaeID;
            if (binderList.Count() > 1)
            {
                for (int i = binderList.Count() - 1; i > 0; i--)
                {
                    if (ID == binderList[i].ID)
                    {
                        return false;
                    }
                    else if (binderList[i].ID < ID)
                    {
                        Console.WriteLine($"TAE file added ID: {taeID.ToString("00")}");
                        binderList.Insert(i + 1, newBinderFile);
                        return true;
                    }
                }
            }
            else if (binderList.Count() == 1)
            {
                if (ID == binderList.First().ID)
                {
                    return false;
                }
                else if (binderList.First().ID < ID)
                {
                    Console.WriteLine($"TAE file added ID: {taeID.ToString("00")}");
                    binderList.Insert(1, newBinderFile);
                    return true;
                }
            }
            return false;
        }
        static bool InsertAfterFirstLowerIDAnimation(int taeSubID, List<TAE3.Animation> animationList, TAE3.Animation newAnimation)
        {
            int ID = taeSubID;
            if (animationList.Count() > 1)
            {
                for (int i = animationList.Count() - 1; i > 0; i--)
                {
                    if (ID == animationList[i].ID)
                    {
                        Console.WriteLine($"TAE subID is already registered ID: {taeSubID.ToString("000000")}");
                        return false;
                    }
                    else if (animationList[i].ID < ID)
                    {
                        Console.WriteLine($"TAE subID registered ID: {taeSubID.ToString("000000")}");
                        animationList.Insert(i + 1, newAnimation);
                        return true;
                    }
                }
            }
            else if (animationList.Count() == 1)
            {
                if (ID == animationList.First().ID)
                {
                    Console.WriteLine($"TAE subID is already registered ID: {taeSubID.ToString("000000")}");
                    return false;
                }
                else if (animationList.First().ID < ID)
                {
                    Console.WriteLine($"TAE subID registered ID: {taeSubID.ToString("000000")}");
                    animationList.Insert(1, newAnimation);
                    return true;
                }
            }
            return false;
        }
    }
}
