namespace Friendship.Domain;

/// <summary>Lỗi vi phạm quy tắc nghiệp vụ; Message là tiếng Việt, hiển thị thẳng cho người dùng.</summary>
public class NghiepVuException(string message) : Exception(message);
