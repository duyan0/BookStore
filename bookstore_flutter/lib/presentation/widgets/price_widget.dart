import 'package:flutter/material.dart';
import '../../core/constants/app_colors.dart';
import '../../core/utils/currency_formatter.dart';

class PriceWidget extends StatelessWidget {
  final double price;
  final double? originalPrice;
  final bool showDiscount;
  final TextStyle? priceStyle;
  final TextStyle? originalPriceStyle;
  final TextStyle? discountStyle;
  final MainAxisAlignment alignment;

  const PriceWidget({
    super.key,
    required this.price,
    this.originalPrice,
    this.showDiscount = true,
    this.priceStyle,
    this.originalPriceStyle,
    this.discountStyle,
    this.alignment = MainAxisAlignment.start,
  });

  @override
  Widget build(BuildContext context) {
    final hasDiscount =
        showDiscount &&
        originalPrice != null &&
        originalPrice! > price &&
        originalPrice! > 0;

    return Row(
      mainAxisAlignment: alignment,
      crossAxisAlignment: CrossAxisAlignment.baseline,
      textBaseline: TextBaseline.alphabetic,
      mainAxisSize: MainAxisSize.min,
      children: [
        // Current price
        Flexible(
          child: Text(
            CurrencyFormatter.formatVNDWithSymbol(price),
            style:
                priceStyle ??
                Theme.of(context).textTheme.titleLarge?.copyWith(
                  color:
                      hasDiscount
                          ? AppColors.discountRed
                          : AppColors.primaryText,
                  fontWeight: FontWeight.bold,
                ),
            overflow: TextOverflow.ellipsis,
          ),
        ),

        if (hasDiscount) ...[
          const SizedBox(width: 4),

          // Original price (strikethrough)
          Flexible(
            child: Text(
              CurrencyFormatter.formatVNDWithSymbol(originalPrice!),
              style:
                  originalPriceStyle ??
                  Theme.of(context).textTheme.bodyMedium?.copyWith(
                    color: AppColors.secondaryText,
                    decoration: TextDecoration.lineThrough,
                    decorationColor: AppColors.secondaryText,
                  ),
              overflow: TextOverflow.ellipsis,
            ),
          ),

          const SizedBox(width: 4),

          // Discount percentage
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 4, vertical: 1),
            decoration: BoxDecoration(
              color: AppColors.discountBackground,
              borderRadius: BorderRadius.circular(3),
              border: Border.all(color: AppColors.discountRed, width: 1),
            ),
            child: Text(
              '-${CurrencyFormatter.formatDiscountPercentage(CurrencyFormatter.calculateDiscountPercentage(originalPrice!, price))}',
              style:
                  discountStyle ??
                  Theme.of(context).textTheme.labelSmall?.copyWith(
                    color: AppColors.discountRed,
                    fontWeight: FontWeight.bold,
                    fontSize: 10,
                  ),
            ),
          ),
        ],
      ],
    );
  }
}

class SimplePriceWidget extends StatelessWidget {
  final double price;
  final TextStyle? style;
  final bool showSymbol;

  const SimplePriceWidget({
    super.key,
    required this.price,
    this.style,
    this.showSymbol = true,
  });

  @override
  Widget build(BuildContext context) {
    return Text(
      showSymbol
          ? CurrencyFormatter.formatVNDWithSymbol(price)
          : CurrencyFormatter.formatVND(price),
      style:
          style ??
          Theme.of(
            context,
          ).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w600),
    );
  }
}

class PriceSummaryWidget extends StatelessWidget {
  final double subTotal;
  final double shippingFee;
  final double discount;
  final double total;
  final EdgeInsets? padding;

  const PriceSummaryWidget({
    super.key,
    required this.subTotal,
    required this.shippingFee,
    required this.discount,
    required this.total,
    this.padding,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: padding ?? const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.lightGray,
        borderRadius: BorderRadius.circular(8),
      ),
      child: Column(
        children: [
          _buildRow(
            context,
            'Tạm tính',
            CurrencyFormatter.formatVNDWithSymbol(subTotal),
          ),
          const SizedBox(height: 8),
          _buildRow(
            context,
            'Phí vận chuyển',
            CurrencyFormatter.formatVNDWithSymbol(shippingFee),
          ),
          if (discount > 0) ...[
            const SizedBox(height: 8),
            _buildRow(
              context,
              'Giảm giá',
              '-${CurrencyFormatter.formatVNDWithSymbol(discount)}',
              valueColor: AppColors.success,
            ),
          ],
          const Divider(height: 24),
          _buildRow(
            context,
            'Tổng cộng',
            CurrencyFormatter.formatVNDWithSymbol(total),
            labelStyle: Theme.of(
              context,
            ).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
            valueStyle: Theme.of(context).textTheme.titleLarge?.copyWith(
              fontWeight: FontWeight.bold,
              color: AppColors.primary,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildRow(
    BuildContext context,
    String label,
    String value, {
    TextStyle? labelStyle,
    TextStyle? valueStyle,
    Color? valueColor,
  }) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(
          label,
          style: labelStyle ?? Theme.of(context).textTheme.bodyMedium,
        ),
        Text(
          value,
          style:
              valueStyle ??
              Theme.of(context).textTheme.bodyMedium?.copyWith(
                fontWeight: FontWeight.w600,
                color: valueColor,
              ),
        ),
      ],
    );
  }
}
