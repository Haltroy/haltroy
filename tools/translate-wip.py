from re import T
from googletrans import Translator
from googletrans import LANGUAGES

translator = Translator()

lines = ['Output path was empty.','No arguments given.','File [0] does not exists.','Input was empty.','Unknown error code [1].',  'Decimal <-> Hexadecimal Converter[NEWLINE]Converts Decimal to Hexadecimal and vice-versa.[NEWLINE]Made by haltroy.[NEWLINE][URL][NEWLINE][NEWLINE]USAGE:[NEWLINE][CMD][NEWLINE]Input[NEWLINE]Output Path[NEWLINE][V][SPC]Verboses the output to console.[NEWLINE][S][SPC]Switches mode to Hexadecimal -> Decimal.[NEWLINE][I][SPC]Changes Input from path to string.[NEWLINE][H][SPC]Prints this output to console and quits.[NEWLINE]Input[NEWLINE]File or string that contains the decimals seperated with a single space.Important.[NEWLINE]Output Path[NEWLINE]File that will contain the conversion result.']

template = "case \"[0]\":\nswitch (index)\n{\n    case 0:\n        return \"[1]\";\n\n    case 1:\n        return \"[2]\";\n\n    case 2:\n        return \"[3]\";\n\n    case 3:\n        return \"[4]\";\n\n    default:\n        return \"[5]\";\n}"
template2 = "case \"[0]\":\nreturn \"[1]\";"

translate1 = ""

translate2 = ""

current = 0

for lang in LANGUAGES:
    if (lang != 'en' and lang != 'tr'):
        result = translator.translate(lines, dest=lang, src='en')
        translate1 = translate1 + template.replace('[0]', lang).replace('[1]', result[0].text).replace('[2]', result[1].text).replace('[3]', result[2].text).replace('[4]', result[3].text).replace('[5]', result[4].text)
        translate2 = translate2 + template2.replace('[0]', lang).replace('[1]', result[5].text.replace('[URL]', 'https://github.com/haltroy/dec2hex').replace('[CMD]','d2h [-v] [-i] [-s] [help|--help|/?|-h|-help] ').replace('[V]','-v').replace('[S]','-s').replace('[I]','-i').replace('[SPC]','                ').replace('[H]','help --help -help /? -h'))
        current = current + 1
        print('DONE: ' + lang + ' '  + str(current) + '/105')
        
with open('t1.txt', 'x', encoding='utf-16') as f:
    f.write(translate1)

with open('t2.txt', 'x', encoding='utf-16') as f2:
    f2.write(translate2)