using System.IO;
using System.Security.Cryptography;

namespace FileDeduplicator.Services
{
    /// <summary>
    /// 全都是假异步方法
    /// </summary>
    public class HashCalculatorService
    {
        // MD5
        public static async Task<byte[]> ComputeMd5Async(Stream stream, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var md5 = MD5.Create();
            return await md5.ComputeHashAsync(stream, cancellationToken);
        }

        public static async Task<byte[]> ComputeFileMd5Async(string filePath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, options: FileOptions.Asynchronous);
            return await ComputeMd5Async(stream, cancellationToken);
        }

        // SHA1
        public static async Task<byte[]> ComputeSha1Async(Stream stream, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var sha1 = SHA1.Create();
            return await sha1.ComputeHashAsync(stream, cancellationToken);
        }

        public static async Task<byte[]> ComputeFileSha1Async(string filePath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, options: FileOptions.Asynchronous);
            return await ComputeSha1Async(stream, cancellationToken);
        }

        // SHA256
        public static async Task<byte[]> ComputeSha256Async(Stream stream, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var sha256 = SHA256.Create();
            return await sha256.ComputeHashAsync(stream, cancellationToken);
        }

        public static async Task<byte[]> ComputeFileSha256Async(string filePath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, options: FileOptions.Asynchronous);
            return await ComputeSha256Async(stream, cancellationToken);
        }

        // SHA384
        public static async Task<byte[]> ComputeSha384Async(Stream stream, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var sha384 = SHA384.Create();
            return await sha384.ComputeHashAsync(stream, cancellationToken);
        }

        public static async Task<byte[]> ComputeFileSha384Async(string filePath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, options: FileOptions.Asynchronous);
            return await ComputeSha384Async(stream, cancellationToken);
        }

        // SHA512
        public static async Task<byte[]> ComputeSha512Async(Stream stream, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var sha512 = SHA512.Create();
            return await sha512.ComputeHashAsync(stream, cancellationToken);
        }

        public static async Task<byte[]> ComputeFileSha512Async(string filePath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, options: FileOptions.Asynchronous);
            return await ComputeSha512Async(stream, cancellationToken);
        }
    }
}
