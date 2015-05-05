#!/bin/bash
xbuild ../Pash.proj /p:Configuration=Release

dir="../Source/PashConsole/bin/Release/"
exe="${dir}/Pash.exe"
dlls="${dir}/*.dll"

if [ "$1" = "-32bit" ]; then
	export AS="as -arch i386"
	export CC="cc -arch i386 -lobjc -liconv -framework Foundation"
fi
mkbundle --deps --static -o Pash $exe $dlls
