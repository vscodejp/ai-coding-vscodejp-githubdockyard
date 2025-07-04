using Survey.Models;
using System.ComponentModel.DataAnnotations;

namespace Survey.Services;

/// <summary>
/// アンケートデータのバリデーションサービス
/// </summary>
public class SurveyValidationService
{
    /// <summary>
    /// アンケートリクエストの詳細バリデーション
    /// </summary>
    /// <param name="request">バリデーション対象のリクエスト</param>
    /// <returns>バリデーション結果（エラーメッセージのリスト）</returns>
    public static List<string> ValidateRequest(SurveyRequest request)
    {
        var errors = new List<string>();

        // 基本的なバリデーション属性チェック
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        
        if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
        {
            errors.AddRange(validationResults.Select(vr => vr.ErrorMessage ?? "バリデーションエラー"));
        }

        // コミュニティ所属のカスタムバリデーション
        if (request.CommunityAffiliation != null)
        {
            var invalidCommunities = request.CommunityAffiliation
                .Where(c => !string.IsNullOrEmpty(c) && !CommunityConstants.AllowedCommunities.Contains(c))
                .ToList();
                
            if (invalidCommunities.Any())
            {
                errors.Add($"無効なコミュニティが指定されています: {string.Join(", ", invalidCommunities)}");
            }
        }

        // 職種のカスタムバリデーション
        if (request.JobRole != null && request.JobRole.Length > 0)
        {
            var invalidJobRoles = request.JobRole
                .Where(jr => !string.IsNullOrEmpty(jr) && !JobRoleConstants.AllowedJobRoles.Contains(jr))
                .ToList();
                
            if (invalidJobRoles.Any())
            {
                errors.Add($"無効な職種が指定されています: {string.Join(", ", invalidJobRoles)}");
            }

            // 「その他」が選択されている場合のJobRoleOtherチェック
            if (request.JobRole.Contains("その他"))
            {
                if (string.IsNullOrWhiteSpace(request.JobRoleOther))
                {
                    errors.Add("職種で「その他」を選択した場合、具体的な職種の入力が必要です");
                }
            }
        }

        return errors;
    }

    /// <summary>
    /// リクエストから保存用エンティティに変換
    /// </summary>
    /// <param name="request">リクエストデータ</param>
    /// <returns>保存用エンティティ</returns>
    public static SurveyEntity ConvertToEntity(SurveyRequest request)
    {
        var now = DateTime.UtcNow;
        return new SurveyEntity
        {
            Id = Guid.NewGuid().ToString(),
            CommunityAffiliation = request.CommunityAffiliation ?? Array.Empty<string>(),
            JobRole = request.JobRole ?? Array.Empty<string>(),
            JobRoleOther = request.JobRoleOther,
            EventRating = request.EventRating,
            Feedback = request.Feedback ?? string.Empty,
            CreatedAt = now,
            UpdatedAt = now,
            EventDate = now.ToString("yyyy-MM-dd")
        };
    }
}
