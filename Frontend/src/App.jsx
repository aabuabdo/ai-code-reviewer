import { useState } from "react";

function App() {
  const [code, setCode] = useState("");
  const [result, setResult] = useState(null);

  const analyze = async () => {
    const res = await fetch("http://localhost:5201/api/review", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ code }),
    });
    setResult(await res.json());
  };

  return (
    <div className="p-4 max-w-xl mx-auto">
      <h1 className="text-2xl font-bold mb-4">AI Code Reviewer</h1>
      <textarea
        className="border w-full h-40 p-2"
        placeholder="Paste your code here..."
        value={code}
        onChange={(e) => setCode(e.target.value)}
      />
      <button
        className="mt-2 bg-blue-600 text-white px-4 py-2"
        onClick={analyze}
      >
        Analyze
      </button>

      {result && (
        <div className="mt-4 bg-gray-100 p-3 rounded">
          <h2 className="font-semibold">Summary:</h2>
          <p>{result.summary}</p>
          <h3 className="mt-2 font-semibold">Suggestions:</h3>
          <ul>
            {result.suggestions.map((s, i) => (
              <li key={i}>
                Line {s.line}: {s.message}
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}

export default App;
