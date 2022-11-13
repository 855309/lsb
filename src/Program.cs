using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace lsb{
    class Program{
        
        static void Main(string[] args){
            if(args[0] == "hide"){
                string fpath = args[1];
                string impath = args[2];

                byte[] file = File.ReadAllBytes(fpath);
                var img = Image.Load<Rgba32>(impath);

                int allocbits = img.Width * img.Height * 6; // 6 bits per pixel
                int filebits = file.Length * 8;

                if(filebits > allocbits){
                    Console.WriteLine($"{filebits} > {allocbits} END. Resolution is not enough for this file.");
                    Console.WriteLine("To calculate the max. file size (mb) for resolution: (width * height * 3) / 8388608");
                    Environment.Exit(1);
                }

                List<byte> data = file.ToList();
                if((file.Length / 2) % 3 != 0){
                    Enumerable.Repeat(() => {
                        data.Add(0);
                    }, (file.Length / 2) % 3);
                } // fill by mod 3

                int x = 0, y = 0, v = 0;
                for(int i = 0; i < data.Count(); i++){
                    string b = Convert.ToString(data[i], 2).PadLeft(8, '0');

                    for(int j = 0; j < 4; j++){
                        string cbits = b.Substring(2 * j, 2);

                        if(v == 0){
                            img[x, y] = new Rgba32{
                                R = Convert.ToByte(Convert.ToString(img[x, y].R, 2).PadLeft(8, '0').Substring(0, 6) + cbits, 2),
                                G = img[x, y].G,
                                B = img[x, y].B,
                                A = img[x, y].A,
                            };
                        }
                        else if(v == 1){
                            img[x, y] = new Rgba32{
                                R = img[x, y].R,
                                G = Convert.ToByte(Convert.ToString(img[x, y].G, 2).PadLeft(8, '0').Substring(0, 6) + cbits, 2),
                                B = img[x, y].B,
                                A = img[x, y].A,
                            };
                        }
                        else if(v == 2){
                            img[x, y] = new Rgba32{
                                R = img[x, y].R,
                                G = img[x, y].G,
                                B = Convert.ToByte(Convert.ToString(img[x, y].B, 2).PadLeft(8, '0').Substring(0, 6) + cbits, 2),
                                A = img[x, y].A,
                            };
                        }
                        
                        v++;
                        if(v == 3){ x++; v = 0; }

                        if(x == img.Width){ y++; x = 0; }
                    }
                }

                //Console.WriteLine($"EndPixel: [{x}, {y}]");
                // Console.WriteLine($"EndPixel: [{filebytes % img.Width}, {(filebytes - (filebytes % img.Width)) / img.Width}]");
                Console.WriteLine($"EndBit: {filebits}");

                img.SaveAsPng(args[3]);
            }
            else if(args[0] == "extract"){
                string impath = args[1];
                int filebits = Convert.ToInt32(args[2]);

                var img = Image.Load<Rgba32>(impath);
                byte[] data = new byte[filebits / 8];
                
                int x = 0, y = 0, v = 0;
                for(int i = 0; i < filebits / 8; i++){
                    string b = "";

                    for(int j = 0; j < 4; j++){
                        if(v == 0){
                            b += Convert.ToString(img[x, y].R, 2).PadLeft(8, '0').Substring(6, 2);
                        }
                        else if(v == 1){
                            b += Convert.ToString(img[x, y].G, 2).PadLeft(8, '0').Substring(6, 2);
                        }
                        else if(v == 2){
                            b += Convert.ToString(img[x, y].B, 2).PadLeft(8, '0').Substring(6, 2);
                        }
                        
                        v++;
                        if(v == 3){ x++; v = 0; }

                        if(x == img.Width){ y++; x = 0; }
                    }

                    data[i] = Convert.ToByte(b, 2);
                }

                File.WriteAllBytes(args[3], data);
            }
        }
    }
}
