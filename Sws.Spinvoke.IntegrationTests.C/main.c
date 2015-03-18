#include "main.h"
#include <stdlib.h>

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