import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:go_router/go_router.dart';
import '../../../core/constants/app_strings.dart';
import '../../../core/constants/app_colors.dart';
import '../../providers/author_provider.dart';
import '../../widgets/loading_widget.dart';
import '../../widgets/error_widget.dart' as custom_error;
import '../../widgets/empty_state_widget.dart';
import '../../../data/models/author_model.dart';

class AdminAuthorsPage extends StatefulWidget {
  const AdminAuthorsPage({super.key});

  @override
  State<AdminAuthorsPage> createState() => _AdminAuthorsPageState();
}

class _AdminAuthorsPageState extends State<AdminAuthorsPage> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _loadAuthors();
    });
  }

  Future<void> _loadAuthors() async {
    final authorProvider = context.read<AuthorProvider>();
    await authorProvider.loadAuthors();
  }

  Future<void> _showCreateAuthorDialog() async {
    final fullNameController = TextEditingController();
    final biographyController = TextEditingController();

    final result = await showDialog<bool>(
      context: context,
      builder:
          (context) => AlertDialog(
            title: const Text('Thêm tác giả mới'),
            content: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                TextField(
                  controller: fullNameController,
                  decoration: const InputDecoration(
                    labelText: 'Tên tác giả',
                    border: OutlineInputBorder(),
                  ),
                ),
                const SizedBox(height: 16),
                TextField(
                  controller: biographyController,
                  decoration: const InputDecoration(
                    labelText: 'Tiểu sử',
                    border: OutlineInputBorder(),
                  ),
                  maxLines: 3,
                ),
              ],
            ),
            actions: [
              TextButton(
                onPressed: () => Navigator.of(context).pop(false),
                child: const Text('Hủy'),
              ),
              ElevatedButton(
                onPressed: () {
                  if (fullNameController.text.trim().isEmpty) {
                    return; // Don't close dialog if validation fails
                  }
                  Navigator.of(context).pop(true);
                },
                child: const Text('Thêm'),
              ),
            ],
          ),
    );

    if (result == true && mounted) {
      // Validate input
      if (fullNameController.text.trim().isEmpty) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Vui lòng nhập tên tác giả'),
            backgroundColor: AppColors.error,
          ),
        );
        return;
      }

      try {
        // Split fullName into firstName and lastName
        final fullName = fullNameController.text.trim();
        final nameParts = fullName.split(' ');
        final firstName = nameParts.isNotEmpty ? nameParts.first : '';
        final lastName =
            nameParts.length > 1 ? nameParts.sublist(1).join(' ') : '';

        final authorData = CreateAuthorModel(
          firstName: firstName,
          lastName: lastName,
          biography: biographyController.text.trim(),
        );

        final success = await context.read<AuthorProvider>().createAuthor(
          authorData,
        );

        if (mounted) {
          if (success) {
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(
                content: Text('Thêm tác giả thành công'),
                backgroundColor: AppColors.success,
              ),
            );
          } else {
            final errorMessage =
                context.read<AuthorProvider>().errorMessage ?? 'Có lỗi xảy ra';
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text('Lỗi khi thêm tác giả: $errorMessage'),
                backgroundColor: AppColors.error,
              ),
            );
          }
        }
      } catch (e) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text('Lỗi khi thêm tác giả: ${e.toString()}'),
              backgroundColor: AppColors.error,
            ),
          );
        }
      }
    }
  }

  Future<void> _showEditAuthorDialog(AuthorModel author) async {
    final fullNameController = TextEditingController(text: author.fullName);
    final biographyController = TextEditingController(text: author.biography);

    final result = await showDialog<bool>(
      context: context,
      builder:
          (context) => AlertDialog(
            title: const Text('Sửa tác giả'),
            content: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                TextField(
                  controller: fullNameController,
                  decoration: const InputDecoration(
                    labelText: 'Tên tác giả',
                    border: OutlineInputBorder(),
                  ),
                ),
                const SizedBox(height: 16),
                TextField(
                  controller: biographyController,
                  decoration: const InputDecoration(
                    labelText: 'Tiểu sử',
                    border: OutlineInputBorder(),
                  ),
                  maxLines: 3,
                ),
              ],
            ),
            actions: [
              TextButton(
                onPressed: () => Navigator.of(context).pop(false),
                child: const Text('Hủy'),
              ),
              ElevatedButton(
                onPressed: () {
                  if (fullNameController.text.trim().isEmpty) {
                    return; // Don't close dialog if validation fails
                  }
                  Navigator.of(context).pop(true);
                },
                child: const Text('Cập nhật'),
              ),
            ],
          ),
    );

    if (result == true && mounted) {
      // Validate input
      if (fullNameController.text.trim().isEmpty) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Vui lòng nhập tên tác giả'),
            backgroundColor: AppColors.error,
          ),
        );
        return;
      }

      try {
        // Split fullName into firstName and lastName
        final fullName = fullNameController.text.trim();
        final nameParts = fullName.split(' ');
        final firstName = nameParts.isNotEmpty ? nameParts.first : '';
        final lastName =
            nameParts.length > 1 ? nameParts.sublist(1).join(' ') : '';

        final authorData = UpdateAuthorModel(
          firstName: firstName,
          lastName: lastName,
          biography: biographyController.text.trim(),
        );

        final success = await context.read<AuthorProvider>().updateAuthor(
          author.id,
          authorData,
        );

        if (mounted) {
          if (success) {
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(
                content: Text('Cập nhật tác giả thành công'),
                backgroundColor: AppColors.success,
              ),
            );
          } else {
            final errorMessage =
                context.read<AuthorProvider>().errorMessage ?? 'Có lỗi xảy ra';
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text('Lỗi khi cập nhật tác giả: $errorMessage'),
                backgroundColor: AppColors.error,
              ),
            );
          }
        }
      } catch (e) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text('Lỗi khi cập nhật tác giả: ${e.toString()}'),
              backgroundColor: AppColors.error,
            ),
          );
        }
      }
    }
  }

  Future<void> _deleteAuthor(AuthorModel author) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder:
          (context) => AlertDialog(
            title: const Text('Xác nhận xóa'),
            content: Text(
              'Bạn có chắc chắn muốn xóa tác giả "${author.fullName}"?',
            ),
            actions: [
              TextButton(
                onPressed: () => Navigator.of(context).pop(false),
                child: const Text('Hủy'),
              ),
              ElevatedButton(
                onPressed: () => Navigator.of(context).pop(true),
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.error,
                  foregroundColor: AppColors.white,
                ),
                child: const Text('Xóa'),
              ),
            ],
          ),
    );

    if (confirmed == true && mounted) {
      try {
        final success = await context.read<AuthorProvider>().deleteAuthor(
          author.id,
        );

        if (mounted) {
          if (success) {
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(
                content: Text('Xóa tác giả thành công'),
                backgroundColor: AppColors.success,
              ),
            );
          } else {
            final errorMessage =
                context.read<AuthorProvider>().errorMessage ?? 'Có lỗi xảy ra';
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text('Lỗi khi xóa tác giả: $errorMessage'),
                backgroundColor: AppColors.error,
              ),
            );
          }
        }
      } catch (e) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text('Lỗi khi xóa tác giả: ${e.toString()}'),
              backgroundColor: AppColors.error,
            ),
          );
        }
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.scaffoldBackground,
      appBar: AppBar(
        title: const Text(AppStrings.authorManagement),
        elevation: 1,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () => context.pop(),
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.add),
            onPressed: _showCreateAuthorDialog,
          ),
          IconButton(icon: const Icon(Icons.refresh), onPressed: _loadAuthors),
        ],
      ),
      body: Consumer<AuthorProvider>(
        builder: (context, authorProvider, child) {
          if (authorProvider.isLoading) {
            return const LoadingWidget();
          }

          if (authorProvider.hasError) {
            return custom_error.ErrorDisplayWidget(
              message: authorProvider.errorMessage ?? 'Có lỗi xảy ra',
              onRetry: _loadAuthors,
            );
          }

          if (authorProvider.isEmpty) {
            return EmptyStateWidget(
              icon: Icons.person_outline,
              title: 'Chưa có tác giả nào',
              subtitle: 'Thêm tác giả đầu tiên để bắt đầu',
            );
          }

          return RefreshIndicator(
            onRefresh: _loadAuthors,
            child: ListView.builder(
              padding: const EdgeInsets.all(16),
              itemCount: authorProvider.authors.length,
              itemBuilder: (context, index) {
                final author = authorProvider.authors[index];
                return _AuthorCard(
                  author: author,
                  onEdit: () => _showEditAuthorDialog(author),
                  onDelete: () => _deleteAuthor(author),
                );
              },
            ),
          );
        },
      ),
    );
  }
}

class _AuthorCard extends StatelessWidget {
  final AuthorModel author;
  final VoidCallback onEdit;
  final VoidCallback onDelete;

  const _AuthorCard({
    required this.author,
    required this.onEdit,
    required this.onDelete,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        author.fullName,
                        style: const TextStyle(
                          fontSize: 18,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      if (author.biography.isNotEmpty) ...[
                        const SizedBox(height: 4),
                        Text(
                          author.biography,
                          style: const TextStyle(
                            color: AppColors.secondaryText,
                          ),
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                        ),
                      ],
                    ],
                  ),
                ),
                PopupMenuButton<String>(
                  onSelected: (value) {
                    switch (value) {
                      case 'edit':
                        onEdit();
                        break;
                      case 'delete':
                        onDelete();
                        break;
                    }
                  },
                  itemBuilder:
                      (context) => [
                        const PopupMenuItem(
                          value: 'edit',
                          child: Row(
                            children: [
                              Icon(Icons.edit, size: 20),
                              SizedBox(width: 8),
                              Text('Sửa'),
                            ],
                          ),
                        ),
                        const PopupMenuItem(
                          value: 'delete',
                          child: Row(
                            children: [
                              Icon(
                                Icons.delete,
                                size: 20,
                                color: AppColors.error,
                              ),
                              SizedBox(width: 8),
                              Text(
                                'Xóa',
                                style: TextStyle(color: AppColors.error),
                              ),
                            ],
                          ),
                        ),
                      ],
                ),
              ],
            ),
            const SizedBox(height: 12),
            Row(
              children: [
                Container(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 8,
                    vertical: 4,
                  ),
                  decoration: BoxDecoration(
                    color: AppColors.primary.withValues(alpha: 0.1),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Text(
                    '${author.bookCount} sách',
                    style: const TextStyle(
                      fontSize: 12,
                      color: AppColors.primary,
                      fontWeight: FontWeight.w500,
                    ),
                  ),
                ),
                const Spacer(),
                Text(
                  'ID: ${author.id}',
                  style: const TextStyle(
                    fontSize: 12,
                    color: AppColors.secondaryText,
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
