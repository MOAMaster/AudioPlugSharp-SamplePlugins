set source="D:\VSTPlugins2019\vst3sdkRev\WPFExample\bin\Release\net5.0-windows\"
set dest="C:\Program Files\Common Files\VST3\WPFExample\"
set DLLs=%source%*.dll
set JSONs=%source%*.json
set VSTs=%source%*.vst3
del /Q %dest%*.*
copy %DLLs% %dest%
copy %JSONs% %dest%
copy %VSTs% %dest%
pause