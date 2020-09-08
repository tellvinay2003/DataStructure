using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructure
{
    public class ClsSorting
    {
        public int[] InsertionSort(int[] input)
        {
            //9, 8, 4, 7, 3, 6, 5, 2, 1
            for (int i = 1; i < input.Length; i++)       // n times
            {
                int temp = input[i];
                int j = i - 1;
                while (j >= 0 && input[j] > temp)        // in worst case n times; in best case the array is sorted then 1
                {
                    input[j + 1] = input[j];
                    j--;
                }                                        // complexity in worst case O(n^2)   in best case it's O(n)
                input[j + 1] = temp;
            }

            return input;
        }


        public int[] BubbleSort(int[] input)
        {
            for (int i = 0; i < input.Length - 1; i++)
            {
                int temp = 0;
                for (int j = 0; j < input.Length - 1; j++)
                {
                    if (input[j] > input[j + 1])
                    {
                        temp = input[j];
                        input[j] = input[j + 1];
                        input[j + 1] = temp;
                    }
                }
            }
            return input;
        }

        public int[] BubbleSortOptimized(int[] input)
        {
            for (int i = 0; i < input.Length - 1; i++)
            {
                int temp = 0;
                for (int j = 0; j < input.Length - 1 - i; j++)
                {
                    if (input[j] > input[j + 1])
                    {
                        temp = input[j];
                        input[j] = input[j + 1];
                        input[j + 1] = temp;
                    }
                }
            }
            return input;
        }

    }
}
