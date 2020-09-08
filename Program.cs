using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructure
{
    class Program
    {
        static void Main(string[] args)
        {
            ClsSorting clsSorting = new ClsSorting();
            var input = new int[] { 9, 8, 4, 7, 3, 6, 5, 2, 1 };
            var x = new int[9, 8, 7, 6, 5, 4, 3, 2, 1];
            var resultInsertion = clsSorting.InsertionSort(input);
            var resultBubble = clsSorting.BubbleSort(input);
            var resultBubbleOptimized = clsSorting.BubbleSortOptimized(input);
        }
    }
}
