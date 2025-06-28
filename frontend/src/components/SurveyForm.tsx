import React from "react";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import {
  Box,
  Button,
  TextField,
  Typography,
  Checkbox,
  FormControlLabel,
  FormGroup,
  FormLabel,
  Radio,
  RadioGroup,
} from "@mui/material";
import type { Survey } from "../types/survey";

// Zodスキーマ

const communityAffiliationOptions = ["VS Code Meetup", "GitHub dockyard"];
const jobRoleOptions = [
  "フロントエンドエンジニア",
  "バックエンドエンジニア",
  "フルスタックエンジニア",
  "DevOpsエンジニア",
  "データエンジニア",
  "モバイルエンジニア",
  "その他",
];

const schema = z.object({
  communityAffiliation: z
    .array(z.enum(["VS Code Meetup", "GitHub dockyard"]))
    .optional()
    .default([]),
  jobRole: z
    .array(
      z.enum([
        "フロントエンドエンジニア",
        "バックエンドエンジニア",
        "フルスタックエンジニア",
        "DevOpsエンジニア",
        "データエンジニア",
        "モバイルエンジニア",
        "その他",
      ])
    )
    .min(1, "1つ以上選択してください"),
  eventRating: z.union([
    z.literal(1),
    z.literal(2),
    z.literal(3),
    z.literal(4),
    z.literal(5),
  ]),
  jobRoleOther: z.string().max(100).optional(),
  feedback: z.string().max(1000).optional(),
});

type FormValues = z.infer<typeof schema>;

export const SurveyForm: React.FC<{
  onSubmit: (data: Survey) => void;
  loading?: boolean;
}> = ({ onSubmit, loading }) => {
  const {
    control,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      communityAffiliation: [],
      jobRole: [],
      eventRating: 3,
      jobRoleOther: "",
      feedback: "",
    },
  });

  const jobRole = watch("jobRole");

  return (
    <Box component="form" onSubmit={handleSubmit(onSubmit)}>
      <FormLabel>コミュニティ所属（複数選択可）</FormLabel>
      <FormGroup row>
        {communityAffiliationOptions.map((c) => (
          <Controller
            key={c}
            name="communityAffiliation"
            control={control}
            render={({ field }) => (
              <FormControlLabel
                control={
                  <Checkbox
                    checked={field.value?.includes(c) ?? false}
                    onChange={(e) => {
                      if (e.target.checked)
                        field.onChange([...(field.value || []), c]);
                      else
                        field.onChange(
                          (field.value || []).filter((v: string) => v !== c)
                        );
                    }}
                  />
                }
                label={c}
              />
            )}
          />
        ))}
      </FormGroup>
      {errors.communityAffiliation && (
        <Typography color="error">
          {errors.communityAffiliation.message}
        </Typography>
      )}

      <FormLabel sx={{ mt: 2 }}>職種（1つ以上必須）</FormLabel>
      <FormGroup row>
        {jobRoleOptions.map((role) => (
          <Controller
            key={role}
            name="jobRole"
            control={control}
            render={({ field }) => (
              <FormControlLabel
                control={
                  <Checkbox
                    checked={field.value.includes(role)}
                    onChange={(e) => {
                      if (e.target.checked)
                        field.onChange([...field.value, role]);
                      else
                        field.onChange(
                          field.value.filter((v: string) => v !== role)
                        );
                    }}
                  />
                }
                label={role}
              />
            )}
          />
        ))}
      </FormGroup>
      {errors.jobRole && (
        <Typography color="error">{errors.jobRole.message}</Typography>
      )}

      {jobRole.includes("その他") && (
        <Controller
          name="jobRoleOther"
          control={control}
          rules={{ required: true, maxLength: 100 }}
          render={({ field }) => (
            <TextField
              label="その他の職種"
              fullWidth
              margin="normal"
              inputProps={{ maxLength: 100 }}
              error={!!errors.jobRoleOther}
              helperText={errors.jobRoleOther?.message || "100字以内"}
              {...field}
            />
          )}
        />
      )}

      <FormLabel sx={{ mt: 2 }}>イベント評価（1-5）</FormLabel>
      <Controller
        name="eventRating"
        control={control}
        render={({ field }) => (
          <RadioGroup
            row
            {...field}
            value={String(field.value)}
            onChange={(e) => field.onChange(Number(e.target.value))}
          >
            {[1, 2, 3, 4, 5].map((n) => (
              <FormControlLabel
                key={n}
                value={String(n)}
                control={<Radio />}
                label={String(n)}
              />
            ))}
          </RadioGroup>
        )}
      />
      {errors.eventRating && (
        <Typography color="error">{errors.eventRating.message}</Typography>
      )}

      <Controller
        name="feedback"
        control={control}
        render={({ field }) => (
          <TextField
            label="フィードバック（任意）"
            fullWidth
            margin="normal"
            multiline
            minRows={3}
            inputProps={{ maxLength: 1000 }}
            error={!!errors.feedback}
            helperText={errors.feedback?.message || "1000字以内"}
            {...field}
          />
        )}
      />

      <Button
        type="submit"
        variant="contained"
        color="primary"
        disabled={loading}
        sx={{ mt: 2 }}
      >
        送信
      </Button>
    </Box>
  );
};
