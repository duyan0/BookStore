import 'package:flutter/material.dart';
import '../../core/constants/app_colors.dart';
import '../../data/models/category_model.dart';

class CategoryChipWidget extends StatelessWidget {
  final CategoryModel category;
  final VoidCallback? onTap;
  final bool isSelected;

  const CategoryChipWidget({
    super.key,
    required this.category,
    this.onTap,
    this.isSelected = false,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        decoration: BoxDecoration(
          color: isSelected ? AppColors.primary : AppColors.lightGray,
          borderRadius: BorderRadius.circular(20),
          border: Border.all(
            color: isSelected ? AppColors.primary : AppColors.borderGray,
          ),
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(
              category.name,
              style: Theme.of(context).textTheme.bodySmall?.copyWith(
                color: isSelected ? AppColors.white : AppColors.primaryText,
                fontWeight: FontWeight.w500,
              ),
            ),
            if (category.bookCount > 0) ...[
              const SizedBox(width: 4),
              Text(
                '(${category.bookCount})',
                style: Theme.of(context).textTheme.bodySmall?.copyWith(
                  color: isSelected ? AppColors.white : AppColors.secondaryText,
                  fontSize: 10,
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}

class CategoryListWidget extends StatelessWidget {
  final List<CategoryModel> categories;
  final int? selectedCategoryId;
  final Function(CategoryModel?)? onCategorySelected;
  final bool showAll;

  const CategoryListWidget({
    super.key,
    required this.categories,
    this.selectedCategoryId,
    this.onCategorySelected,
    this.showAll = true,
  });

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 40,
      child: ListView.builder(
        scrollDirection: Axis.horizontal,
        padding: const EdgeInsets.symmetric(horizontal: 16),
        itemCount: categories.length + (showAll ? 1 : 0),
        itemBuilder: (context, index) {
          if (showAll && index == 0) {
            return Padding(
              padding: const EdgeInsets.only(right: 8),
              child: CategoryChipWidget(
                category: CategoryModel(
                  id: 0,
                  name: 'Tất cả',
                  description: '',
                  bookCount: 0,
                  createdAt: DateTime.now(),
                ),
                isSelected: selectedCategoryId == null,
                onTap: () => onCategorySelected?.call(null),
              ),
            );
          }

          final categoryIndex = showAll ? index - 1 : index;
          final category = categories[categoryIndex];

          return Padding(
            padding: const EdgeInsets.only(right: 8),
            child: CategoryChipWidget(
              category: category,
              isSelected: selectedCategoryId == category.id,
              onTap: () => onCategorySelected?.call(category),
            ),
          );
        },
      ),
    );
  }
}
