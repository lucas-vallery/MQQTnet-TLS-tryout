# MQTTnet TLS encryption tryout

## Disclaimer
This project goal is to figure out if both standard TCP connections and TLS encrypted connections are possible at the same time on different ports to the same broker.
It was extensively written with Copilot and copied from [MQTTnet examples files](https://github.com/dotnet/MQTTnet/tree/master/Samples). It is NOT meant to be clean code.

## Conslusion
✅ MQTTnet 5.0.1 does support two connections (a standard and an encrypted one) on two differents ports at the same time.