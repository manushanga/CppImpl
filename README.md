READ ME
-------

Generates the CPP for header files. Uses Clang under the hood, so you don't have to worry about 
it accidentally generating wrong signatures.

Optionally, you may add a file with include directories (one per line) at %USERPROFILE%/.CppImpl/include.txt

Input: Header.h
```c++
namespace ss1
{
    namespace ss2
    {
        class A
        {
        public:
            A();
            virtual ~A();
            void foo();
            int getFoo()
            {
                return 1;
            }
        private:
            void bar();

        };
    }
}
```
Output: Header.cpp
```c++
#include <Header.h> 

ss1::ss2::A::A()
{

}

ss1::ss2::A::~A()
{

}

void ss1::ss2::A::foo()
{

}

void ss1::ss2::A::bar()
{

}
```
