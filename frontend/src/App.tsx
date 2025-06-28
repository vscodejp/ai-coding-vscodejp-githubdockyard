import React, { useState } from "react";
import { Home } from "./pages/Home";
import { Results } from "./pages/Results";
import { Container, AppBar, Toolbar, Typography, Button } from "@mui/material";

function App() {
  const [page, setPage] = useState<"home" | "results">("home");

  return (
    <Container maxWidth="md">
      <AppBar position="static" sx={{ mb: 4 }}>
        <Toolbar>
          <Typography variant="h6" sx={{ flexGrow: 1 }}>
            アンケートアプリ
          </Typography>
          <Button color="inherit" onClick={() => setPage("home")}>
            アンケート
          </Button>
          <Button color="inherit" onClick={() => setPage("results")}>
            集計結果
          </Button>
        </Toolbar>
      </AppBar>
      {page === "home" ? <Home /> : <Results />}
    </Container>
  );
}

export default App;
