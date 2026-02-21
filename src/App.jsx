import "./App.css";
import { useState, useEffect } from "react";

function App() {
  const [tarefas, setTarefas] = useState([]);
  const [valorInput, setValorInput] = useState("");

  const listar = async () => {
    const res = await fetch("http://localhost:5137/tarefas");
    const dados = await res.json();
    setTarefas(dados);
  };

  const post = async () => {
    if (!valorInput.trim()) return alert("Você precisa digitar algo!");

    try {
      const res = await fetch("http://localhost:5137/tarefas", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          titulo: valorInput,
          concluida: false,
          emAndamento: false,
        }),
      });

      if (res.ok) {
        setValorInput("");
        listar();
      }
    } catch (error) {
      console.error("Erro ao salvar tarefa:", error);
    }
  };

  useEffect(() => {
    listar();
  }, []);

  const deletar = async (id) => {
    const confirmacao = window.confirm("Tem certeza que deseja excluir?");

    if (!confirmacao) return;

    await fetch(`http://localhost:5137/tarefas/${id}`, { method: "DELETE" });

    listar();
  };

  async function movertarefa(tarefa, paraConcluido, paraEmAndamento) {
    const payload = {
      ...tarefa,
      concluida: paraConcluido,
      emAndamento: paraEmAndamento,
    };

    try {
      const res = await fetch(`http://localhost:5137/tarefas/${tarefa.id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });

      if (res.ok) {
        listar();
      }
    } catch (error) {
      alert("Erro ao mover tarefa", error);
    }
  }

  const handleDragStart = (e, tarefaId) => {
    e.dataTransfer.setData("tarefaId", tarefaId);
  };

  const handleDrop = (e, destinoConcluido, destinoEmAndamento) => {
    e.preventDefault();
    const id = e.dataTransfer.getData("tarefaId");
    const tarefaParaMover = tarefas.find((t) => t.id === parseInt(id));

    if (tarefaParaMover) {
      movertarefa(tarefaParaMover, destinoConcluido, destinoEmAndamento);
    }
  };

  function Tarefa({ tarefa, onDelete, onMove }) {
    const cor = tarefa.concluida
      ? "green"
      : tarefa.emAndamento
        ? "yellow"
        : "red";

    return (
      <li
        className="li_style"
        draggable
        onDragStart={(e) => handleDragStart(e, tarefa.id)}
        style={{ backgroundColor: cor }}
      >
        <span>{tarefa.titulo}</span>
        <button onClick={() => onDelete(tarefa.id)}>❌</button>
      </li>
    );
  }

  return (
    <main>
      <h1>TO-DO DASHBOARD</h1>
      <div className="div_input">
        <input
          type="text"
          className="input_value"
          value={valorInput}
          onChange={(e) => setValorInput(e.target.value)}
        />{" "}
        <button onClick={post} className="btn_adicionar">
          Adicionar
        </button>
      </div>
      <div className="div_section">
        <section
          onDragOver={(e) => e.preventDefault()}
          onDrop={(e) => handleDrop(e, false, false)}
        >
          <h3>A Fazer</h3>
          <ul className="ul_1">
            {tarefas
              .filter((t) => !t.concluida && !t.emAndamento)
              .map((t) => (
                <Tarefa key={t.id} tarefa={t} onDelete={deletar} />
              ))}
          </ul>
        </section>

        <section
          onDragOver={(e) => e.preventDefault()}
          onDrop={(e) => handleDrop(e, false, true)}
        >
          <h3>Em Andamento</h3>
          <ul className="ul_emAndamento">
            {tarefas
              .filter((t) => t.emAndamento && !t.concluida)
              .map((t) => (
                <Tarefa key={t.id} tarefa={t} onDelete={deletar} />
              ))}
          </ul>
        </section>

        <section
          onDragOver={(e) => e.preventDefault()}
          onDrop={(e) => handleDrop(e, true, false)}
        >
          <h3>Concluido</h3>
          <ul className="ul_concluido">
            {tarefas
              .filter((t) => t.concluida)
              .map((t) => (
                <Tarefa key={t.id} tarefa={t} onDelete={deletar} />
              ))}
          </ul>
        </section>
      </div>
    </main>
  );
}

export default App;
