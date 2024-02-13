#include <stdlib.h>
#include <stdio.h>

int main(int argc, // Number of strings in array argv
    char* argv[])      // Array of command-line argument strings
{
    int count;

    // variable to store size of Arr
    int length = sizeof(argv) / sizeof(argv[0]);

    printf("The length of the array is: %d\n", length);

    // Display each command-line argument.
    printf_s("\n%d Command-line arguments:\n", argc);
    for (count = 0; count < argc; count++)
        printf_s("  argv[%d]   %s\n", count, argv[count]);

    return;
}