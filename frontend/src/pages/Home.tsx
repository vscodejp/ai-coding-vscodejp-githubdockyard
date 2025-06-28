import React, { useState } from "react";
import { SurveyForm } from "../components/SurveyForm";
import { postSurvey } from "../api/surveys";
import type { Survey } from "../types/survey";
import { Box, Typography, Alert } from "@mui/material";

export const Home: React.FC = () => {
  const [submitted, setSubmitted] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (data: Survey) => {
    setLoading(true);
    setError(null);
    try {
      const res = await postSurvey(data);
      if (res.success) {
        setSubmitted(true);
      } else {
        setError(res.message || res.error || "送信に失敗しました");
      }
    } catch (e) {
      setError("送信時にエラーが発生しました");
    } finally {
      setLoading(false);
    }
  };

  if (submitted) {
    return (
      <Box sx={{ mt: 4 }}>
        <Typography variant="h5" color="primary">
          ご回答ありがとうございました！
        </Typography>
      </Box>
    );
  }

  return (
    <Box
      sx={{
        width: "100%",
        maxWidth: { xs: "100%", sm: 600, md: 800 },
        mx: "auto",
        mt: 4,
        px: { xs: 1, sm: 2, md: 0 },
        boxSizing: "border-box",
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
      }}
    >
      <Typography variant="h4" gutterBottom align="center">
        アンケート
      </Typography>
      {error && <Alert severity="error">{error}</Alert>}
      <Box sx={{ width: "100%" }}>
        <SurveyForm onSubmit={handleSubmit} loading={loading} />
      </Box>
    </Box>
  );
};
