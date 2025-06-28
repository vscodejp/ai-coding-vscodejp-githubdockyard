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
const schema = z.object({
  communityAffiliation: z.array(z.string()),
  jobRole: z.array(z.string()).min(1, "1つ以上選択してください"),
  eventRating: z.number().min(1).max(5),
  jobRoleOther: z.string().max(100).optional(),
  feedback: z.string().max(1000).optional(),
});

type FormValues = z.infer<typeof schema>;

const jobRoles = ["エンジニア", "デザイナー", "PM", "その他"];
const communities = ["コミュニティA", "コミュニティB", "コミュニティC"];

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
        {communities.map((c) => (
          <Controller
            key={c}
            name="communityAffiliation"
            control={control}
            render={({ field }) => (
              <FormControlLabel
                control={
                  <Checkbox
                    checked={field.value.includes(c)}
                    onChange={(e) => {
                      if (e.target.checked) field.onChange([...field.value, c]);
                      else
                        field.onChange(
                          field.value.filter((v: string) => v !== c)
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
        {jobRoles.map((role) => (
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
