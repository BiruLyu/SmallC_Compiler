void opt(int a, int b)
{
int c,d,e;
d=a;
e=b;
while(a!=b)
{
	if(a>b)
	{
		c=a;
		a=b;
		b=c;
	}
	b=b-a;
}
c=a;
a=d/c;
b=e/c;
c=a*b*c;
write c;
}


void main()
{
int a,b;
read a;
read b;
call opt(a,b);
}.