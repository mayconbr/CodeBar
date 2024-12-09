# Projeto de Leitura de Códigos de Barras para Estoque

## Descrição

Este projeto consiste em uma API desenvolvida com **ASP.NET** que recebe imagens de códigos de barras, realiza a leitura e insere os dados no banco de dados. A solução foi idealizada para ser implementada em um sistema de controle de estoque. Futuramente, será integrada com um **ESP32** com câmera, que enviará automaticamente as imagens para a API por meio de uma esteira de produção.

## Funcionalidades

- Recepção de imagens contendo códigos de barras.
- Leitura do código de barras nas imagens enviadas.
- Armazenamento das informações extraídas em um banco de dados.
- Integração futura com **ESP32** para captura e envio de imagens diretamente da esteira.

## Tecnologias Utilizadas

- **ASP.NET Core** para a construção da API.
- **Banco de Dados**: MySQL
- **HTML, CSS, JS e Bootstrap
- **ESP32**: Para captura e envio das imagens em uma esteira (futuro).
