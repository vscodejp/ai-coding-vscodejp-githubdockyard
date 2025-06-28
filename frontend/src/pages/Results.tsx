import React from "react";
import { useQuery } from "@tanstack/react-query";
import { getSurveyResults } from "../api/surveys";
import { SurveyResult } from "../components/SurveyResult";
import { Box, Typography, CircularProgress, Alert } from "@mui/material";

export const Results: React.FC = () => {
  const { data, isLoading, error } = useQuery({
    queryKey: ["surveyResults"],
    queryFn: getSurveyResults,
  });

  return (
    <Box sx={{ maxWidth: 600, mx: "auto", mt: 4 }}>
      <Typography variant="h4" gutterBottom>
        集計結果
      </Typography>
      {isLoading && <CircularProgress />}
      {error && <Alert severity="error">集計データの取得に失敗しました</Alert>}
      {data && <SurveyResult result={data} />}
    </Box>
  );
};
