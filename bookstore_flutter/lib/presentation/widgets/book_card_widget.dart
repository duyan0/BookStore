import 'package:flutter/material.dart';
import 'package:cached_network_image/cached_network_image.dart';
import '../../core/constants/app_colors.dart';
import '../../core/utils/image_utils.dart';
import '../../data/models/book_model.dart';
import 'price_widget.dart';

class BookCardWidget extends StatelessWidget {
  final BookModel book;
  final VoidCallback? onTap;
  final bool showAddToCart;
  final VoidCallback? onAddToCart;

  const BookCardWidget({
    super.key,
    required this.book,
    this.onTap,
    this.showAddToCart = false,
    this.onAddToCart,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(12),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Book Image
            Expanded(
              flex: 3,
              child: Container(
                width: double.infinity,
                decoration: const BoxDecoration(
                  borderRadius: BorderRadius.vertical(top: Radius.circular(12)),
                ),
                child: ClipRRect(
                  borderRadius: const BorderRadius.vertical(
                    top: Radius.circular(12),
                  ),
                  child: Stack(
                    children: [
                      CachedNetworkImage(
                        imageUrl: ImageUtils.getFullImageUrl(book.imageUrl),
                        width: double.infinity,
                        height: double.infinity,
                        fit: BoxFit.cover,
                        placeholder:
                            (context, url) => Container(
                              color: AppColors.lightGray,
                              child: const Center(
                                child: CircularProgressIndicator(),
                              ),
                            ),
                        errorWidget:
                            (context, url, error) => Container(
                              color: AppColors.lightGray,
                              child: const Center(
                                child: Icon(
                                  Icons.book,
                                  size: 48,
                                  color: AppColors.secondaryText,
                                ),
                              ),
                            ),
                      ),

                      // Discount badge
                      if (book.hasDiscount)
                        Positioned(
                          top: 8,
                          right: 8,
                          child: Container(
                            padding: const EdgeInsets.symmetric(
                              horizontal: 6,
                              vertical: 2,
                            ),
                            decoration: BoxDecoration(
                              color: AppColors.discountRed,
                              borderRadius: BorderRadius.circular(4),
                            ),
                            child: Text(
                              '-${((book.originalPrice - book.effectivePrice) / book.originalPrice * 100).round()}%',
                              style: const TextStyle(
                                color: AppColors.white,
                                fontSize: 10,
                                fontWeight: FontWeight.bold,
                              ),
                            ),
                          ),
                        ),

                      // Stock status
                      if (!book.isInStock)
                        Positioned.fill(
                          child: Container(
                            decoration: BoxDecoration(
                              color: Colors.black.withValues(alpha: 0.6),
                              borderRadius: const BorderRadius.vertical(
                                top: Radius.circular(12),
                              ),
                            ),
                            child: const Center(
                              child: Text(
                                'Hết hàng',
                                style: TextStyle(
                                  color: AppColors.white,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            ),
                          ),
                        ),
                    ],
                  ),
                ),
              ),
            ),

            // Book Info
            Expanded(
              flex: 2,
              child: Padding(
                padding: const EdgeInsets.all(12),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    // Title
                    Flexible(
                      child: Text(
                        book.title,
                        style: Theme.of(context).textTheme.titleSmall?.copyWith(
                          fontWeight: FontWeight.w600,
                        ),
                        maxLines: 2,
                        overflow: TextOverflow.ellipsis,
                      ),
                    ),
                    const SizedBox(height: 4),

                    // Author
                    Text(
                      book.authorName,
                      style: Theme.of(context).textTheme.bodySmall?.copyWith(
                        color: AppColors.secondaryText,
                      ),
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                    const Spacer(),

                    // Price
                    Flexible(
                      child: PriceWidget(
                        price: book.effectivePrice,
                        originalPrice:
                            book.hasDiscount ? book.originalPrice : null,
                        priceStyle: Theme.of(
                          context,
                        ).textTheme.titleSmall?.copyWith(
                          fontWeight: FontWeight.bold,
                          color:
                              book.hasDiscount
                                  ? AppColors.discountRed
                                  : AppColors.primaryText,
                        ),
                        originalPriceStyle: Theme.of(
                          context,
                        ).textTheme.bodySmall?.copyWith(
                          color: AppColors.secondaryText,
                          decoration: TextDecoration.lineThrough,
                        ),
                        alignment: MainAxisAlignment.start,
                      ),
                    ),

                    // Add to Cart Button
                    if (showAddToCart && book.isInStock) ...[
                      const SizedBox(height: 8),
                      SizedBox(
                        width: double.infinity,
                        child: ElevatedButton(
                          onPressed: onAddToCart,
                          style: ElevatedButton.styleFrom(
                            padding: const EdgeInsets.symmetric(vertical: 8),
                            textStyle: const TextStyle(fontSize: 12),
                          ),
                          child: const Text('Thêm vào giỏ'),
                        ),
                      ),
                    ],
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
