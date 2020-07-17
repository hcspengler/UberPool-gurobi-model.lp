# UberPool-gurobi-model.lp
Modelo (linear programming) em Gurobi, da Formulação matemática do Modelo de Dial-a-Ride, para ambientes de compartilhamento de viagens (Uber Pool).

# Referência da Formulação Matemática
A formulação matemática 3-index provem do trabalho "A Branch-and-Cut Algorithm for the Dial-a-Ride Problem" de JEAN-FRANÇOIS CORDEAU.
O modelo implementado não inclui as desigualdades válidas utilizadas para o Branch-and-Cut também apresentado no trabalho referido.
Além disso, o autor J.-F. Cordeau disponibiliza as instâncias com os valores para cada teste.

# Dial-a-Ride Problem
O problema DARP procura encontrar rotas de mínimo custo para um ambiente, em que há n pedidos de viagens para serem atendidos por uma frota de k veículos. Neste ambiente é permitido que haja compartilhamento da viagens entre os usuários, ainda que regidos por restrições quanto a janela de tempo (entre os pontos de embarque e desembarque), capacidade de atendimento para cada veículo e tempo de viagem para os usuários e motoristas.

# Estrutura
A programação é disponibilizada em uma Classe feita em c# (visual studio), com o apoio do software Gurobi para escrita de modelos lineares.
Inicialmente, é utilizado structs para facilitar a notação dos valores do problema.
Há Métodos Públicos para Leitura das Instâncias (em .txt), Ajuste de Janelas de tempo (de acordo com o trabalho referência) e para o próprio modelo.
Ao fim, junto ao modelo, há alguns processos para conferir a existência da solução e escrever um .txt como relatório.
