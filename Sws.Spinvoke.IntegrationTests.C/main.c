#include "main.h"
#include <stdlib.h>
#include <string.h>

int add(int x, int y)
{
	return x + y;
}

int* pointerAdd(int* x, int* y)
{
	int* ret = malloc(sizeof(int));

	*ret = *x + *y;

	return ret;
}

char* reverseString(char* input)
{
	int len = strlen(input);

	char* ret = malloc((len + 1) * sizeof(char));

	int index;

	for (index = 0; index < len; index++)
	{
		int newIndex = (len - 1) - index;

		ret[newIndex] = input[index];
	}

	ret[len] = '\0';

	return ret;
}