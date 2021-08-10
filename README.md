# AniBNDRegister
Barebones Command Line tool to register stuff in anibnd, ideal to use in your own programs or .bat scripts

:AnibndPath :Comma Separated list of TAE ids to register :Comma separated list of animation tae subids to register

Mass edit mode:
Usage AniBNDRegister.exe "C:\GamePath\chr\c0000.anibnd.dcx" "0,2,3,4,5,6,20,650,660" "000000,000300,000301,030000,030010,030020,033000,033010,033020,034000,034010,034020"

Single Edit mode:
Usage AniBNDRegister.exe "C:\GamePath\chr\c0000.anibnd.dcx" "999" "030000"

