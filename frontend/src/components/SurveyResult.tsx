import React from "react";
import {
  Box,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
} from "@mui/material";
import type { SurveyResult as SurveyResultType } from "../types/survey";

export const SurveyResult: React.FC<{ result: SurveyResultType }> = ({
  result,
}) => {
  return (
    <Box>
      <Typography variant="h6" sx={{ mb: 2 }}>
        集計結果
      </Typography>
      <Typography>総回答数: {result.total}</Typography>
      <Typography variant="subtitle1" sx={{ mt: 2 }}>
        職種別集計
      </Typography>
      <TableContainer component={Paper} sx={{ mb: 2 }}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>職種</TableCell>
              <TableCell>人数</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {Object.entries(result.jobRoleStats).map(([role, count]) => (
              <TableRow key={role}>
                <TableCell>{role}</TableCell>
                <TableCell>{count}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
      <Typography variant="subtitle1">イベント評価</Typography>
      <TableContainer component={Paper}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>評価</TableCell>
              <TableCell>人数</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {Object.entries(result.eventRatingStats).map(([rating, count]) => (
              <TableRow key={rating}>
                <TableCell>{rating}</TableCell>
                <TableCell>{count}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
};
