class AppBreakpoints {
  static const double mobile = 640;
  static const double tablet = 1024;
  static const double desktop = 1280;

  static bool isMobile(double width) => width < mobile;

  static bool isTablet(double width) => width >= mobile && width < tablet;

  static bool isDesktop(double width) => width >= tablet;

  static int gridColumns(double width) {
    if (width >= desktop) {
      return 4;
    }

    if (width >= tablet) {
      return 3;
    }

    if (width >= mobile) {
      return 2;
    }

    return 1;
  }

  static double pagePadding(double width) {
    if (width >= desktop) {
      return 28;
    }

    if (width >= mobile) {
      return 22;
    }

    return 16;
  }
}
