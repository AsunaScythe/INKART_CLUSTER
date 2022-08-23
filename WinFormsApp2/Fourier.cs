using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp2
{
    internal class Fourier
    {
        static public double[] Four(double[] signal,int K){
            double a = 0,
            b = 0,
            size = signal.Length;
            for(int i = 0;i<size;i++){
                a += signal[i]*Math.Cos(2*Math.PI/size* i*K);
                b += signal[i]*Math.Sin(2* Math.PI/size* i*K);
            }
            a/=size;
            b/=size;
            double[] temp = new double[2];
            temp[0] = a;
            temp[1] = b;

            return temp;
        }
        static public double[] Bandpass_filter(double[] signal, int frequency, double min_freq, double max_freq)
        {
            int N = signal.Length;
            double time = (double)N / frequency;
            double[] filtered_signal = new double[N];
            for (int i = 0; i < N; i++)
                filtered_signal[i] = 0;

            for (int i = (int)(min_freq * time); i < max_freq * time; i++)
            {
                double[] temp = Four(signal, i);
                for (int j = 0; j < N; j++)
                {
                    filtered_signal[j] += temp[0] * Math.Cos(Math.PI * 2 / N * i * j) + temp[1] * Math.Sin(Math.PI * 2 / N * i * j);
                }
            }
            return filtered_signal;
        }

        static public double[] Difference(double[] signal)
        {
            double[] diff_signal = new double[signal.Length];
            for(int i = 0;i < signal.Length-1; i++)
            {
                diff_signal[i] = signal[i + 1] - signal[i];
            }
            diff_signal[diff_signal.Length - 1] = diff_signal[diff_signal.Length - 2];
            return diff_signal;
        }

        static public double[] Square(double[] signal)
        {
            double[] square_signal = signal;
            for (int i = 0; i < square_signal.Length; i++)
            {
                square_signal[i] *= square_signal[i];
            }
            return square_signal;
        }

        static public double[] Normalization(double[] signal)
        {
            double sum = 0;
            for(int i = 0;i < signal.Length; i++)
            {
                sum += signal[i];
            }
            double mean = sum / signal.Length;

            double min = signal[0],
                   max = 0; 

            double[] answer = new double[signal.Length];
            for(int i = 0;i < signal.Length; i++)
            {
                answer[i] = signal[i] - mean;
                if(max < answer[i])  max = answer[i];

                if (min > answer[i]) min = answer[i];
            }

            double height = max - min;
            for(int i = 0;i < signal.Length; i++)
            {
                answer[i] = (answer[i] -  min)/height;
            }

            return answer;
        }

        static public double[] Integration(double[] signal)
        {
            double[] answer = new double[signal.Length];
           
            double sum = 0;
            for(int i =0;i < 12; i++)
            {
                sum += signal[i];
                
            }
            for(int i = 12,n = 0;i < 24; i++,n++)
            {
                answer[i] = sum / n;
                sum += signal[i];
            }
            for(int i = 12;i < signal.Length - 12; i++)
            {
                answer[i] = sum / 24;
                sum = sum - signal[i-12] + signal[i + 12];
            }
            for(int i = signal.Length-12,n = 24;i < signal.Length; i++,n--)
            {
                answer[i] = sum / n;
                sum -= signal[i - 12];
            }
            return answer;
        }
        
        static public int[] Maximum(double[] signal)
        {
            double max = 0;
            int index = 0;
            for(int i = 0;i < 100; i++)
            {
                if (signal[i] > max)
                {
                    max = signal[i];
                    index = i;
                }
            }
            List<int> MAX = new();
            int n = 0;
            for(int i = 100;i < signal.Length; i++)
            {
                if (signal[i]> max)
                {
                    index = i;
                    max = signal[i];
                    n = 0;
                }
                else
                {
                    n++;
                    if(n == 100)
                    {
                        MAX.Add(index);
                        n = 0;
                        max = 0;
                    }
                }
            }
            MAX.Add(index);
            return MAX.ToArray();
        }

        static public int[] AAMT(double[] signal)
        {
            int[] MAX = Maximum(signal);

            double T = 0;

            for (int i = 0; i < MAX.Length; i++) T += signal[MAX[i]];

            double M = T/MAX.Length,
                       R = 0.6 * T / MAX.Length,
                       Vsl = 0,
                       Vnl = 0;

            List<int> G = new(),
                      J = new();

            double mean_freq = 0;


            for (int i = 0; i < MAX.Length; i++)
            {
                if (signal[MAX[i]] > M * 8){
                    G.Add(MAX[i]);
                }
                else
                {
                    if (signal[MAX[i]] > M)
                    {
                        G.Add(MAX[i]);
                        if (G.Count > 1) mean_freq += MAX[i] - MAX[i - 1];
                        Vsl = Vsl * 0.9 + signal[MAX[i]] * 0.1;
                    }
                    else if (signal[MAX[i]] < M)
                    {
                        J.Add(MAX[i]);
                        Vnl = Vnl * 0.8 + signal[MAX[i]] * 0.2;
                    }
                    else
                    {
                        if (G.Count > 1)
                        {
                            if (MAX[i] - G[G.Count - 1] >  (mean_freq / (G.Count - 1))/1.75)
                            {
                                G.Add(MAX[i]);
                                mean_freq += MAX[i] - MAX[i - 1];
                                Vsl = Vsl * 0.9 + signal[MAX[i]] * 0.1;
                            }
                            else
                            {
                                J.Add(MAX[i]);
                                Vnl = Vnl * 0.8 + signal[MAX[i]] * 0.2;
                            }
                        }
                        else
                        {
                            J.Add(MAX[i]);
                            Vnl = Vnl * 0.8 + signal[MAX[i]] * 0.2;
                        }
                    }
                    M = 0.6 * Vnl + 0.6 * (Vsl - Vnl);
                    R = M / 2;
                }
                
            }


            return G.ToArray();
        }
        
    }

    
    
}
